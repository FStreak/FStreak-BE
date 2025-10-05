using FStreak.Domain.Entities;

namespace FStreak.Domain.Interfaces
{
    public interface IStreakRepository : IRepository<StreakLog>
    {
        Task<StreakLog> GetUserStreakForDateAsync(string userId, DateTime date);
        Task<int> GetCurrentStreakAsync(string userId);
        Task<int> GetLongestStreakAsync(string userId);
    }
}