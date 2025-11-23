using FStreak.Application.DTOs;

namespace FStreak.Application.Services.Interface
{
    public interface IAchievementService
    {
        /// <summary>
        /// Get all achievements for a user
        /// </summary>
        Task<Result<List<UserAchievementDto>>> GetUserAchievementsAsync(string userId);

        /// <summary>
        /// Get a specific user achievement
        /// </summary>
        Task<Result<UserAchievementDto>> GetUserAchievementAsync(string userId, Guid achievementId);

        /// <summary>
        /// Claim an achievement (mark as claimed)
        /// </summary>
        Task<Result<UserAchievementDto>> ClaimAchievementAsync(string userId, Guid achievementId);

        /// <summary>
        /// Award an achievement to a user (automatically called when criteria is met)
        /// </summary>
        Task<Result<UserAchievementDto>> AwardAchievementAsync(string userId, string achievementCode, string? progress = null);

        /// <summary>
        /// Get all active achievements
        /// </summary>
        Task<Result<List<AchievementDto>>> GetAllAchievementsAsync();

        /// <summary>
        /// Get achievement by ID
        /// </summary>
        Task<Result<AchievementDto>> GetAchievementByIdAsync(Guid id);

        /// <summary>
        /// Create a new achievement (Admin only)
        /// </summary>
        Task<Result<AchievementDto>> CreateAchievementAsync(CreateAchievementDto dto);

        /// <summary>
        /// Update an achievement (Admin only)
        /// </summary>
        Task<Result<AchievementDto>> UpdateAchievementAsync(Guid id, UpdateAchievementDto dto);

        /// <summary>
        /// Delete an achievement (Admin only)
        /// </summary>
        Task<Result<bool>> DeleteAchievementAsync(Guid id);

        /// <summary>
        /// Activate/Deactivate an achievement (Admin only)
        /// </summary>
        Task<Result<bool>> ToggleAchievementStatusAsync(Guid id, bool isActive);
    }
}

