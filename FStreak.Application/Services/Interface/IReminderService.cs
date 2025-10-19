using FStreak.Application.DTOs;

namespace FStreak.Application.Services.Interface
{
    public interface IReminderService
    {
        Task<List<ReminderDto>> GetUserRemindersAsync(string userId);
        Task<ReminderDto> CreateReminderAsync(string userId, CreateReminderDto dto);
        Task<ReminderDto> UpdateReminderAsync(string userId, int reminderId, UpdateReminderDto dto);
        Task<bool> DeleteReminderAsync(string userId, int reminderId);
        Task<bool> ToggleReminderAsync(string userId, int reminderId, bool enable);
        Task ProcessDueRemindersAsync(DateTime processTime);
    }
}