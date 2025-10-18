using FStreak.Domain.Entities;

namespace FStreak.Domain.Interfaces
{
    public interface IReminderRepository : IRepository<Reminder>
    {
        Task<List<Reminder>> GetUserRemindersAsync(string userId);
        Task<List<Reminder>> GetActiveRemindersForTimeAsync(TimeSpan timeOfDay, DayOfWeek dayOfWeek);
        Task<List<Reminder>> GetDueRemindersAsync(DateTime fromTime, DateTime toTime);
        Task<Reminder?> GetReminderAsync(int reminderId, string userId);
    }
}