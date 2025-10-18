using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using FStreak.Application.DTOs;
using FStreak.Application.Services.Interface;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FStreak.Application.Services.Implementation
{
    internal class TrackingInfo
    {
        public DateTime StartTime { get; set; }
        public string Source { get; set; } = string.Empty;
    }
    public class StreakService : IStreakService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _cache;
        private readonly ILogger<StreakService> _logger;
        private const int DEFAULT_STREAK_THRESHOLD_MINUTES = 25;
        private const string CACHE_KEY_PREFIX = "streak_";
        private const int CACHE_EXPIRY_HOURS = 24;

        public StreakService(
            ILogger<StreakService> logger,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IDistributedCache cache)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _cache = cache;
        }

        public async Task<Result<StreakInfoDto>> GetUserStreakAsync(string userId)
        {
            try
            {
                var user = (await _unitOfWork.Users.FindAsync(u => u.Id == userId)).FirstOrDefault();

                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found", userId);
                    return Result<StreakInfoDto>.Failure("User not found");
                }

                var streakLogs = await _unitOfWork.StreakLogs.FindAsync(s => s.UserId == userId);

                var logs = streakLogs.OrderByDescending(s => s.Date).ToList();
                var currentStreak = CalculateCurrentStreak(logs);
                var longestStreak = CalculateLongestStreak(logs);
                var lastCheckIn = logs.FirstOrDefault()?.Date;

                var streakInfo = new StreakInfoDto
                {
                    UserId = userId,
                    CurrentStreak = currentStreak,
                    LongestStreak = longestStreak,
                    LastCheckInDate = lastCheckIn,
                    TimeZone = user.TimeZone ?? "UTC",
                    StreakHistory = logs.Select(l => l.Date).ToList()
                };

                return Result<StreakInfoDto>.Success(streakInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user streak for {UserId}", userId);
                return Result<StreakInfoDto>.Failure("Failed to get user streak information");
            }
        }

        public async Task<Result<StreakInfoDto>> CheckInAsync(string userId, StreakCheckInDto dto, string idempotencyKey)
        {
            try
            {
                var key = $"{CACHE_KEY_PREFIX}{idempotencyKey}";
                var usedKey = await _cache.GetStringAsync(key);
                if (!string.IsNullOrEmpty(usedKey))
                {
                    _logger.LogWarning("Idempotency key {Key} already used", key);
                    return Result<StreakInfoDto>.Failure("This check-in has already been processed");
                }

                // Check if already checked in for this date
                var existingStreak = (await _unitOfWork.StreakLogs
                    .FindAsync(s => s.UserId == userId && s.Date.Date == dto.Date.Date))
                    .FirstOrDefault();
                
                if (existingStreak != null)
                {
                    _logger.LogWarning("User {UserId} already has a streak log for {Date}", userId, dto.Date.Date);
                    return Result<StreakInfoDto>.Failure("Already checked in for this date");
                }

                // Check study time threshold for group sessions
                if (dto.Source == StreakSource.GroupSession)
                {
                    var requiredMinutes = _configuration.GetValue<int>("StreakThresholdMinutes", DEFAULT_STREAK_THRESHOLD_MINUTES);
                    var studySessions = await _unitOfWork.StudySessions
                        .FindAsync(s => s.HostId == userId && s.StartAt.Date == dto.Date.Date);

                    var totalMinutes = studySessions.Sum(s => s.DurationMinutes);
                    if (totalMinutes < requiredMinutes)
                    {
                        _logger.LogInformation("User {UserId} needs {Required} minutes but only has {Total} minutes", 
                            userId, requiredMinutes, totalMinutes);
                        return Result<StreakInfoDto>.Failure($"Need at least {requiredMinutes} minutes of study time");
                    }
                }

                var streakLog = new StreakLog
                {
                    UserId = userId,
                    Date = dto.Date.Date,
                    Source = dto.Source.ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.StreakLogs.AddAsync(streakLog);
                await _unitOfWork.SaveChangesAsync();

                // Mark idempotency key as used
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CACHE_EXPIRY_HOURS)
                };
                await _cache.SetStringAsync(key, "used", options);

                _logger.LogInformation("Successfully checked in streak for user {UserId} on {Date}", userId, dto.Date.Date);
                return await GetUserStreakAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking in streak for {UserId}", userId);
                return Result<StreakInfoDto>.Failure("Failed to process check-in");
            }
        }

        public async Task<Result<StreakLeaderboardDto>> GetLeaderboardAsync(LeaderboardRequestDto request)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var startDate = request.Period switch
                {
                    LeaderboardPeriod.Week => today.AddDays(-7),
                    LeaderboardPeriod.Month => today.AddMonths(-1),
                    _ => today.AddDays(-7)
                };

                var users = request.Scope == LeaderboardScope.Global ? 
                    await _unitOfWork.Users.GetAllAsync() :
                    await GetGroupMembers(request.GroupId);

                var streakEntries = new List<StreakLeaderboardEntryDto>();
                foreach (var user in users)
                {
                    var streakLogs = await _unitOfWork.StreakLogs
                        .FindAsync(s => s.UserId == user.Id && s.Date >= startDate && s.Date <= today);

                    var currentStreak = CalculateCurrentStreak(streakLogs.ToList());
                    if (currentStreak > 0)
                    {
                        streakEntries.Add(new StreakLeaderboardEntryDto
                        {
                            UserId = user.Id,
                            DisplayName = user.UserName ?? "Unknown",
                            CurrentStreak = currentStreak
                        });
                    }
                }

                var leaderboard = new StreakLeaderboardDto
                {
                    Period = request.Period,
                    Items = streakEntries.OrderByDescending(e => e.CurrentStreak).ToList()
                };

                return Result<StreakLeaderboardDto>.Success(leaderboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting streak leaderboard for scope {Scope}", request.Scope);
                return Result<StreakLeaderboardDto>.Failure("Failed to get leaderboard");
            }
        }

        private async Task<IEnumerable<ApplicationUser>> GetGroupMembers(int? groupId)
        {
            if (!groupId.HasValue)
            {
                _logger.LogDebug("No group ID provided for getting members");
                return Enumerable.Empty<ApplicationUser>();
            }

            var studyGroup = (await _unitOfWork.StudyGroups
                .FindAsync(g => g.StudyGroupId == groupId.Value))
                .FirstOrDefault();

            if (studyGroup == null)
            {
                _logger.LogWarning("Study group {GroupId} not found", groupId.Value);
                return Enumerable.Empty<ApplicationUser>();
            }

            var members = await _unitOfWork.GroupMembers
                .FindAsync(m => m.GroupId == groupId.Value);

            var userIds = members.Select(m => m.UserId).ToList();
            var users = await _unitOfWork.Users
                .FindAsync(u => userIds.Contains(u.Id));

            return users;
        }

        private int CalculateCurrentStreak(List<StreakLog> logs)
        {
            var orderedLogs = logs.OrderByDescending(l => l.Date).ToList();
            if (!orderedLogs.Any())
                return 0;

            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);
            var lastCheckIn = orderedLogs.First().Date.Date;

            // If haven't checked in today or yesterday, streak is broken
            if (lastCheckIn < yesterday)
                return 0;

            var streak = 1;
            var currentDate = lastCheckIn;

            for (var i = 1; i < orderedLogs.Count; i++)
            {
                var prevDate = orderedLogs[i].Date.Date;
                if (currentDate.AddDays(-1) != prevDate)
                    break;

                streak++;
                currentDate = prevDate;
            }

            return streak;
        }

        public async Task<Result<bool>> StartTracking(string userId, string source)
        {
            try
            {
                // Cache key format: "tracking_[userId]_[source]"
                var key = $"tracking_{userId}_{source}";
                var tracking = await _cache.GetStringAsync(key);

                if (!string.IsNullOrEmpty(tracking))
                {
                    _logger.LogWarning("User {UserId} already tracking for source {Source}", userId, source);
                    return Result<bool>.Failure("Already tracking");
                }

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12) // Reasonable maximum session length
                };

                var trackingInfo = new
                {
                    StartTime = DateTime.UtcNow,
                    Source = source
                };

                await _cache.SetStringAsync(key, JsonSerializer.Serialize(trackingInfo), options);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting tracking for user {UserId}", userId);
                return Result<bool>.Failure("Failed to start tracking");
            }
        }

        public async Task<Result<bool>> StopTracking(string userId, string source)
        {
            try
            {
                var key = $"tracking_{userId}_{source}";
                var trackingJson = await _cache.GetStringAsync(key);

                if (string.IsNullOrEmpty(trackingJson))
                {
                    _logger.LogWarning("No active tracking found for user {UserId} and source {Source}", userId, source);
                    return Result<bool>.Failure("No active tracking found");
                }

                var trackingInfo = JsonSerializer.Deserialize<TrackingInfo>(trackingJson);
                if (trackingInfo == null)
                {
                    return Result<bool>.Failure("Invalid tracking data");
                }

                var duration = DateTime.UtcNow - trackingInfo.StartTime;

                // Remove tracking record
                await _cache.RemoveAsync(key);

                // If duration meets threshold, create a streak check-in
                var requiredMinutes = _configuration.GetValue<int>("StreakThresholdMinutes", DEFAULT_STREAK_THRESHOLD_MINUTES);
                if (duration.TotalMinutes >= requiredMinutes)
                {
                    var checkInDto = new StreakCheckInDto
                    {
                        Date = DateTime.UtcNow,
                        Source = Enum.Parse<StreakSource>(source)
                    };

                    var result = await CheckInAsync(userId, checkInDto, Guid.NewGuid().ToString());
                    if (!result.Succeeded)
                    {
                        _logger.LogWarning("Failed to create streak check-in: {Error}", result.Error);
                    }
                }
                else
                {
                    _logger.LogInformation(
                        "Session duration {0} minutes is below threshold {1} minutes",
                        duration.TotalMinutes,
                        requiredMinutes);
                }

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping tracking for user {UserId}", userId);
                return Result<bool>.Failure("Failed to stop tracking");
            }
        }

        private int CalculateLongestStreak(List<StreakLog> logs)
        {
            var orderedLogs = logs.OrderBy(l => l.Date).ToList();
            if (!orderedLogs.Any())
                return 0;

            var longestStreak = 1;
            var currentStreak = 1;
            var currentDate = orderedLogs.First().Date.Date;

            for (var i = 1; i < orderedLogs.Count; i++)
            {
                var nextDate = orderedLogs[i].Date.Date;
                if (currentDate.AddDays(1) == nextDate)
                {
                    currentStreak++;
                    if (currentStreak > longestStreak)
                        longestStreak = currentStreak;
                }
                else
                {
                    currentStreak = 1;
                }
                currentDate = nextDate;
            }

            return longestStreak;
        }
    }
}
