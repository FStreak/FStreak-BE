namespace FStreak.Application.Services.Interface
{
    public interface IStreakRealtimeNotifier
    {
        Task NotifyStreakCheckedIn(string userId, DateTime date);
    }
}
