using FStreak.Application.DTOs;

namespace FStreak.Application.Services.Interface
{
    public interface IStreakService
    {
        /// <summary>
        /// Get the current streak information for a user
        /// </summary>
        Task<Result<StreakInfoDto>> GetUserStreakAsync(string userId);

        /// <summary>
        /// Record a check-in for today's learning activity
        /// </summary>
        Task<Result<StreakInfoDto>> CheckInAsync(string userId, StreakCheckInDto dto, string idempotencyKey);

        /// <summary>
        /// Get the streak leaderboard based on specified scope and period
        /// </summary>
        Task<Result<StreakLeaderboardDto>> GetLeaderboardAsync(LeaderboardRequestDto request);

        /// <summary>
        /// Start tracking a user's study session
        /// </summary>
        Task<Result<bool>> StartTracking(string userId, string source);

        /// <summary>
        /// Stop tracking a user's study session
        /// </summary>
        Task<Result<bool>> StopTracking(string userId, string source);
    }
}