using Microsoft.EntityFrameworkCore;
using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using FStreak.Infrastructure.Data;

namespace FStreak.Infrastructure.Repositories
{
    public class ReminderRepository : Repository<Reminder>, IReminderRepository
    {
        public ReminderRepository(FStreakDbContext context) : base(context)
        {
        }

        public async Task<List<Reminder>> GetUserRemindersAsync(string userId)
        {
            return await _dbSet
                .Where(r => r.UserId == userId)
                .OrderBy(r => r.TimeOfDay)
                .ToListAsync();
        }

        public async Task<List<Reminder>> GetActiveRemindersForTimeAsync(TimeSpan timeOfDay, DayOfWeek dayOfWeek)
        {
            var dayNumber = ((int)dayOfWeek + 1).ToString();
            return await _dbSet
                .Where(r => 
                    r.IsEnabled && 
                    r.TimeOfDay == timeOfDay &&
                    r.DaysOfWeek.Contains(dayNumber))
                .Include(r => r.User)
                .ToListAsync();
        }

        public async Task<List<Reminder>> GetDueRemindersAsync(DateTime fromTime, DateTime toTime)
        {
            var currentDayOfWeek = ((int)fromTime.DayOfWeek + 1).ToString();
            
            return await _dbSet
                .Where(r => 
                    r.IsEnabled &&
                    r.DaysOfWeek.Contains(currentDayOfWeek) &&
                    (r.LastTriggered == null || r.LastTriggered.Value.Date < fromTime.Date))
                .Where(r => 
                    r.TimeOfDay >= fromTime.TimeOfDay &&
                    r.TimeOfDay < toTime.TimeOfDay)
                .Include(r => r.User)
                .ToListAsync();
        }

        public async Task<Reminder?> GetReminderAsync(int reminderId, string userId)
        {
            return await _dbSet
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReminderId == reminderId && r.UserId == userId);
        }
    }
}