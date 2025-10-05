namespace FStreak.Application.Services.Interface
{
    public interface IStreakService
    {
        Task StartTracking(int roomId, string userId);
        Task StopTracking(int roomId, string userId);
        Task ProcessStreak(int roomId, string userId);
        Task<(bool success, int currentStreak, int longestStreak)> CheckInAsync(string userId);
        Task<int> GetCurrentStreakAsync(string userId);
        Task<int> GetLongestStreakAsync(string userId);
    }
}