using FStreak.Domain.Entities;

namespace FStreak.Domain.Interfaces
{
    public interface IStreakRepository : IRepository<StreakLog>
    {
        Task<StreakLog> GetUserStreakForDateAsync(int userId, DateTime date);
        Task<int> GetCurrentStreakAsync(int userId);
        Task<int> GetLongestStreakAsync(int userId);
    }
}