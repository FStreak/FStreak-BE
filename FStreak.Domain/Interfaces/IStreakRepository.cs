using FStreak.Domain.Entities;

namespace FStreak.Domain.Interfaces
{
    public interface IStreakRepository : IRepository<StreakLog>
    {
        Task<StreakLog?> GetUserStreakForDateAsync(string userId, DateTime date);
        Task<int> GetCurrentStreakAsync(string userId);
        Task<int> GetLongestStreakAsync(string userId);
        Task<List<StreakLog>> GetStreakHistoryAsync(string userId, DateTime startDate, DateTime endDate);
        Task<List<(ApplicationUser User, int StreakCount)>> GetLeaderboardAsync(DateTime startDate, DateTime endDate, int? groupId = null);
        Task<bool> HasCheckedInTodayAsync(string userId);
        Task<(int CurrentStreak, int LongestStreak)> GetStreakStatsAsync(string userId);
    }
}