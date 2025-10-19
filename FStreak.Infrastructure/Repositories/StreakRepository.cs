using Microsoft.EntityFrameworkCore;
using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using FStreak.Infrastructure.Data;

namespace FStreak.Infrastructure.Repositories
{
    public class StreakRepository : Repository<StreakLog>, IStreakRepository
    {
        public StreakRepository(FStreakDbContext context) : base(context)
        {
        }

        public async Task<StreakLog?> GetUserStreakForDateAsync(string userId, DateTime date)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Date.Date == date.Date);
        }

        private async Task<(int CurrentStreak, int LongestStreak)> CalculateStreakStatsAsync(List<StreakLog> streaks)
        {
            if (!streaks.Any())
                return (0, 0);

            streaks = streaks.OrderBy(s => s.Date).ToList();
            int currentStreak = 1;
            int longestStreak = 1;
            DateTime previousDate = streaks[0].Date;

            for (int i = 1; i < streaks.Count; i++)
            {
                if (streaks[i].Date == previousDate.AddDays(1))
                {
                    currentStreak++;
                    if (currentStreak > longestStreak)
                    {
                        longestStreak = currentStreak;
                    }
                }
                else
                {
                    currentStreak = 1;
                }
                previousDate = streaks[i].Date;
            }

            return (currentStreak, longestStreak);
        }

        public async Task<int> GetCurrentStreakAsync(string userId)
        {
            var lastStreak = await _dbSet
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.Date)
                .FirstOrDefaultAsync();

            if (lastStreak == null || lastStreak.Date.Date < DateTime.UtcNow.Date.AddDays(-1))
                return 0;

            var streaks = await _dbSet
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.Date)
                .ToListAsync();

            var (currentStreak, _) = await CalculateStreakStatsAsync(streaks);
            return currentStreak;
        }

        public async Task<int> GetLongestStreakAsync(string userId)
        {
            var streaks = await _dbSet
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.Date)
                .ToListAsync();

            var (_, longestStreak) = await CalculateStreakStatsAsync(streaks);
            return longestStreak;
        }

        public async Task<List<StreakLog>> GetStreakHistoryAsync(string userId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(s => s.UserId == userId && s.Date >= startDate && s.Date <= endDate)
                .OrderByDescending(s => s.Date)
                .ToListAsync();
        }

        public async Task<(int CurrentStreak, int LongestStreak)> GetStreakStatsAsync(string userId)
        {
            var streaks = await _dbSet
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.Date)
                .ToListAsync();

            return await CalculateStreakStatsAsync(streaks);
        }

        public async Task<bool> HasCheckedInTodayAsync(string userId)
        {
            var today = DateTime.UtcNow.Date;
            return await _dbSet.AnyAsync(s => s.UserId == userId && s.Date.Date == today);
        }

        public async Task<List<(ApplicationUser User, int StreakCount)>> GetLeaderboardAsync(DateTime startDate, DateTime endDate, int? groupId = null)
        {
            var query = _dbSet
                .Include(s => s.User)
                .Where(s => s.Date >= startDate && s.Date <= endDate);

            if (groupId.HasValue)
            {
                // Filter users by group membership
                var groupMemberIds = _context.Set<StudyGroup>()
                    .Where(g => g.StudyGroupId == groupId)
                    .SelectMany(g => g.Members)
                    .Select(m => m.UserId);

                query = query.Where(s => groupMemberIds.Contains(s.UserId));
            }

            var streakCounts = await query
                .GroupBy(s => new { s.User })
                .Select(g => new { User = g.Key.User, StreakCount = g.Count() })
                .OrderByDescending(x => x.StreakCount)
                .Take(100)
                .ToListAsync();

            return streakCounts.Select(x => (x.User, x.StreakCount)).ToList();
        }
    }
}