using FStreak.Domain.Entities;

namespace FStreak.Domain.Interfaces
{
    public interface IAchievementRepository : IRepository<Achievement>
    {
        Task<Achievement?> GetByIdAsync(Guid id);
        Task<Achievement?> GetByCodeAsync(string code);
        Task<bool> ExistsByCodeAsync(string code);
        Task<IEnumerable<Achievement>> GetActiveAchievementsAsync();
        Task<IEnumerable<UserAchievement>> GetUserAchievementsAsync(string userId);
        Task<UserAchievement?> GetUserAchievementAsync(string userId, Guid achievementId);
        Task<bool> UserHasAchievementAsync(string userId, Guid achievementId);
    }
}

