using FStreak.Domain.Interfaces;
using FStreak.Domain.Entities;

namespace FStreak.Application.Services
{
    public interface IStreakService
    {
        Task<(bool success, int currentStreak, int longestStreak)> CheckInAsync(int userId);
        Task<int> GetCurrentStreakAsync(int userId);
        Task<int> GetLongestStreakAsync(int userId);
    }

    public class StreakService : IStreakService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StreakService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool success, int currentStreak, int longestStreak)> CheckInAsync(int userId)
        {
            var today = DateTime.UtcNow.Date;
            var todayStreaks = await _unitOfWork.StreakLogs.FindAsync(s => s.UserId == userId && s.Date.Date == today);

            if (todayStreaks.Any())
            {
                return (false, 0, 0); // Already checked in today
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return (false, 0, 0);
            }

            // Create new streak log
            var streakLog = new StreakLog
            {
                UserId = userId,
                Date = today,
                Minutes = 0, // Initial check-in
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.StreakLogs.AddAsync(streakLog);

            // Check if this continues the streak from yesterday
            var yesterdayStreak = await _unitOfWork.StreakLogs.FindAsync(s => 
                s.UserId == userId && s.Date.Date == today.AddDays(-1));

            if (yesterdayStreak.Any())
            {
                user.CurrentStreak++;
                if (user.CurrentStreak > user.LongestStreak)
                {
                    user.LongestStreak = user.CurrentStreak;
                }
            }
            else
            {
                user.CurrentStreak = 1;
            }

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return (true, user.CurrentStreak, user.LongestStreak);
        }

        public async Task<int> GetCurrentStreakAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            return user?.CurrentStreak ?? 0;
        }

        public async Task<int> GetLongestStreakAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            return user?.LongestStreak ?? 0;
        }
    }
}