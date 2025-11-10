using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using FStreak.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FStreak.Infrastructure.Repositories
{
    public class AchievementRepository : Repository<Achievement>, IAchievementRepository
    {
        private readonly FStreakDbContext _dbContext;

        public AchievementRepository(FStreakDbContext context) : base(context)
        {
            _dbContext = context;
        }

        public async Task<Achievement?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Achievements
                .Include(a => a.UserAchievements)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Achievement?> GetByCodeAsync(string code)
        {
            return await _dbContext.Achievements
                .FirstOrDefaultAsync(a => a.Code == code);
        }

        public async Task<bool> ExistsByCodeAsync(string code)
        {
            return await _dbContext.Achievements
                .AnyAsync(a => a.Code == code);
        }

        public async Task<IEnumerable<Achievement>> GetActiveAchievementsAsync()
        {
            return await _dbContext.Achievements
                .Where(a => a.IsActive)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserAchievement>> GetUserAchievementsAsync(string userId)
        {
            return await _dbContext.UserAchievements
                .Include(ua => ua.Achievement)
                .Where(ua => ua.UserId == userId)
                .OrderByDescending(ua => ua.EarnedAt)
                .ToListAsync();
        }

        public async Task<UserAchievement?> GetUserAchievementAsync(string userId, Guid achievementId)
        {
            return await _dbContext.UserAchievements
                .Include(ua => ua.Achievement)
                .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId);
        }

        public async Task<bool> UserHasAchievementAsync(string userId, Guid achievementId)
        {
            return await _dbContext.UserAchievements
                .AnyAsync(ua => ua.UserId == userId && ua.AchievementId == achievementId);
        }
    }
}

