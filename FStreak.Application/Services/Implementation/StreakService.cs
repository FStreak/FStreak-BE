using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using FStreak.Application.Services.Interface;

namespace FStreak.Application.Services.Implementation
{
    /// <summary>
    /// Service for managing user study streaks, using UnitOfWork, cache, and logging.
    /// </summary>
    public class StreakService : IStreakService
    {
        private const int RequiredMinutes = 10;
        private const string TrackedUserKeyPrefix = "tracked_user_";

        private readonly IDistributedCache _cache;
        private readonly ILogger<StreakService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public StreakService(IDistributedCache cache, ILogger<StreakService> logger, IUnitOfWork unitOfWork)
        {
            _cache = cache;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        // Get cache key for tracking user
        private string GetUserTrackingKey(string userId) => $"{TrackedUserKeyPrefix}{userId}";

        // Get tracking info from cache
        private async Task<StudyTrackingInfo?> GetTrackingInfoAsync(string userId)
        {
            var key = GetUserTrackingKey(userId);
            var trackingJson = await _cache.GetStringAsync(key);
            return string.IsNullOrEmpty(trackingJson)
                ? null
                : JsonSerializer.Deserialize<StudyTrackingInfo>(trackingJson);
        }

        // Save tracking info to cache
        private async Task SaveTrackingInfoAsync(string userId, StudyTrackingInfo info)
        {
            var key = GetUserTrackingKey(userId);
            var trackingJson = JsonSerializer.Serialize(info);
            await _cache.SetStringAsync(key, trackingJson);
        }

        /// <summary>
        /// Start tracking study time for a user in a room.
        /// </summary>
        public async Task StartTracking(int roomId, string userId)
        {
            _logger.LogInformation("Start tracking for user {UserId} in room {RoomId}", userId, roomId);
            var existingTracking = await GetTrackingInfoAsync(userId);
            if (existingTracking != null)
            {
                _logger.LogWarning("User {UserId} is already being tracked in room {RoomId}", userId, existingTracking.RoomId);
                return;
            }
            var trackingInfo = new StudyTrackingInfo
            {
                UserId = userId,
                RoomId = roomId,
                StartTime = DateTime.UtcNow
            };
            await SaveTrackingInfoAsync(userId, trackingInfo);
        }

        /// <summary>
        /// Stop tracking; if enough time is spent, record the streak.
        /// </summary>
        public async Task StopTracking(int roomId, string userId)
        {
            _logger.LogInformation("Stop tracking for user {UserId} in room {RoomId}", userId, roomId);
            var trackingInfo = await GetTrackingInfoAsync(userId);
            if (trackingInfo == null)
            {
                _logger.LogWarning("No tracking info found for user {UserId}", userId);
                return;
            }
            if (trackingInfo.RoomId != roomId)
            {
                _logger.LogWarning("User {UserId} was tracked in a different room {TrackedRoomId}", userId, trackingInfo.RoomId);
                return;
            }
            var studyDuration = DateTime.UtcNow - trackingInfo.StartTime;
            if (studyDuration.TotalMinutes < RequiredMinutes)
            {
                _logger.LogInformation("User {UserId} stayed less than {Minutes} minutes, no streak recorded", userId, RequiredMinutes);
                await _cache.RemoveAsync(GetUserTrackingKey(userId));
                return;
            }
            await ProcessStreak(roomId, userId);
            await _cache.RemoveAsync(GetUserTrackingKey(userId));
        }

        /// <summary>
        /// Record streak for user, update current and longest streak.
        /// </summary>
        public async Task ProcessStreak(int roomId, string userId)
        {
            try
            {
                var user = await _unitOfWork.Users.FindAsync(u => u.UserId.ToString() == userId);
                var userEntity = user.FirstOrDefault();
                if (userEntity == null)
                    throw new Exception($"User {userId} not found");

                var today = DateTime.UtcNow.Date;
                var yesterday = today.AddDays(-1);

                var hasStreakToday = (await _unitOfWork.StreakLogs.FindAsync(s =>
                    s.UserId.ToString() == userId && s.Date.Date == today)).Any();
                if (hasStreakToday)
                {
                    _logger.LogInformation("User {UserId} already has streak for today", userId);
                    return;
                }

                var hasYesterday = (await _unitOfWork.StreakLogs.FindAsync(s =>
                    s.UserId.ToString() == userId && s.Date.Date == yesterday)).Any();
                userEntity.CurrentStreak = hasYesterday ? userEntity.CurrentStreak + 1 : 1;
                if (userEntity.CurrentStreak > userEntity.LongestStreak)
                    userEntity.LongestStreak = userEntity.CurrentStreak;

                var streakLog = new StreakLog
                {
                    UserId = int.Parse(userId),
                    Date = today,
                    Minutes = RequiredMinutes
                };
                await _unitOfWork.StreakLogs.AddAsync(streakLog);
                await _unitOfWork.Users.UpdateAsync(userEntity);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Processed streak for user {UserId} successfully", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing streak for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Manual check-in, return status and streak info.
        /// </summary>
        public async Task<(bool success, int currentStreak, int longestStreak)> CheckInAsync(string userId)
        {
            var user = (await _unitOfWork.Users.FindAsync(u => u.UserId.ToString() == userId)).FirstOrDefault();
            if (user == null) return (false, 0, 0);
            var today = DateTime.UtcNow.Date;
            var hasCheckedIn = (await _unitOfWork.StreakLogs.FindAsync(s =>
                s.UserId.ToString() == userId && s.Date.Date == today)).Any();
            if (hasCheckedIn)
                return (false, user.CurrentStreak, user.LongestStreak);
            await ProcessStreak(0, userId);
            return (true, user.CurrentStreak, user.LongestStreak);
        }

        /// <summary>
        /// Get current streak of user.
        /// </summary>
        public async Task<int> GetCurrentStreakAsync(string userId)
        {
            var user = (await _unitOfWork.Users.FindAsync(u => u.UserId.ToString() == userId)).FirstOrDefault();
            return user?.CurrentStreak ?? 0;
        }

        /// <summary>
        /// Get longest streak of user.
        /// </summary>
        public async Task<int> GetLongestStreakAsync(string userId)
        {
            var user = (await _unitOfWork.Users.FindAsync(u => u.UserId.ToString() == userId)).FirstOrDefault();
            return user?.LongestStreak ?? 0;
        }
    }
}
