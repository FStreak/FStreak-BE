using Microsoft.EntityFrameworkCore;
using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using FStreak.Infrastructure.Data;

namespace FStreak.Infrastructure.Repositories
{
    public class StreakRepository : BaseRepository<StreakLog>, IStreakRepository
    {
        public StreakRepository(FStreakDbContext context) : base(context)
        {
        }

        public async Task<StreakLog> GetUserStreakForDateAsync(int userId, DateTime date)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Date.Date == date.Date);
        }

        public async Task<int> GetCurrentStreakAsync(int userId)
        {
            var lastStreak = await _dbSet
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.Date)
                .FirstOrDefaultAsync();

            if (lastStreak == null || lastStreak.Date.Date < DateTime.UtcNow.Date.AddDays(-1))
                return 0;

            int streakCount = 0;
            var currentDate = DateTime.UtcNow.Date;
            
            while (true)
            {
                currentDate = currentDate.AddDays(-1);
                var streakLog = await _dbSet
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.Date.Date == currentDate);
                
                if (streakLog == null)
                    break;
                    
                streakCount++;
            }

            return streakCount;
        }

        public async Task<int> GetLongestStreakAsync(int userId)
        {
            var streaks = await _dbSet
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.Date)
                .ToListAsync();

            if (!streaks.Any())
                return 0;

            int currentStreak = 1;
            int longestStreak = 1;
            var previousDate = streaks[0].Date;

            for (int i = 1; i < streaks.Count; i++)
            {
                if (streaks[i].Date == previousDate.AddDays(1))
                {
                    currentStreak++;
                    if (currentStreak > longestStreak)
                        longestStreak = currentStreak;
                }
                else
                {
                    currentStreak = 1;
                }
                previousDate = streaks[i].Date;
            }

            return longestStreak;
        }
    }
}