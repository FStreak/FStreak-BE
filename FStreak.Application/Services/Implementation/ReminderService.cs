using Microsoft.Extensions.Logging;
using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using FStreak.Application.DTOs;
using FStreak.Application.Services.Interface;

namespace FStreak.Application.Services.Implementation
{
    public class ReminderService : IReminderService
    {
        private readonly ILogger<ReminderService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IReminderRepository _reminderRepository;
        // TODO: Add INotificationService when implementing notifications
        // private readonly INotificationService _notificationService;

        public ReminderService(
            ILogger<ReminderService> logger,
            IUnitOfWork unitOfWork,
            IReminderRepository reminderRepository)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _reminderRepository = reminderRepository;
        }

        private static ReminderDto ToDto(Reminder reminder)
        {
            return new ReminderDto
            {
                ReminderId = reminder.ReminderId,
                Title = reminder.Title,
                Description = reminder.Description,
                TimeZoneId = reminder.TimeZoneId,
                TimeOfDay = reminder.TimeOfDay,
                DaysOfWeek = reminder.GetDaysOfWeek().Select(d => ((int)d + 1) % 7 + 1).ToList(),
                NotificationChannel = reminder.NotificationChannel,
                IsEnabled = reminder.IsEnabled,
                LastTriggered = reminder.LastTriggered
            };
        }

        public async Task<List<ReminderDto>> GetUserRemindersAsync(string userId)
        {
            var reminders = await _reminderRepository.GetUserRemindersAsync(userId);
            return reminders.Select(ToDto).ToList();
        }

        public async Task<ReminderDto> CreateReminderAsync(string userId, CreateReminderDto dto)
        {
            var reminder = new Reminder
            {
                UserId = userId,
                Title = dto.Title,
                Description = dto.Description,
                TimeZoneId = dto.TimeZoneId,
                TimeOfDay = dto.TimeOfDay,
                NotificationChannel = dto.NotificationChannel,
                CreatedAt = DateTime.UtcNow,
                User = (await _unitOfWork.Users.FindAsync(u => u.Id == userId)).First()
            };

            reminder.SetDaysOfWeek(dto.DaysOfWeek.Select(d => (DayOfWeek)((d - 1) % 7)));
            
            await _reminderRepository.AddAsync(reminder);
            await _unitOfWork.SaveChangesAsync();

            return ToDto(reminder);
        }

        public async Task<ReminderDto> UpdateReminderAsync(string userId, int reminderId, UpdateReminderDto dto)
        {
            var reminder = await _reminderRepository.GetReminderAsync(reminderId, userId);
            if (reminder == null)
            {
                throw new KeyNotFoundException($"Reminder {reminderId} not found for user {userId}");
            }

            if (dto.Title != null) reminder.Title = dto.Title;
            if (dto.Description != null) reminder.Description = dto.Description;
            if (dto.TimeZoneId != null) reminder.TimeZoneId = dto.TimeZoneId;
            if (dto.TimeOfDay.HasValue) reminder.TimeOfDay = dto.TimeOfDay.Value;
            if (dto.NotificationChannel != null) reminder.NotificationChannel = dto.NotificationChannel;
            if (dto.IsEnabled.HasValue) reminder.IsEnabled = dto.IsEnabled.Value;
            if (dto.DaysOfWeek != null) reminder.SetDaysOfWeek(dto.DaysOfWeek.Select(d => (DayOfWeek)((d - 1) % 7)));

            reminder.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.SaveChangesAsync();

            return ToDto(reminder);
        }

        public async Task<bool> DeleteReminderAsync(string userId, int reminderId)
        {
            var reminder = await _reminderRepository.GetReminderAsync(reminderId, userId);
            if (reminder == null) return false;

            await _reminderRepository.DeleteAsync(reminder);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ToggleReminderAsync(string userId, int reminderId, bool enable)
        {
            var reminder = await _reminderRepository.GetReminderAsync(reminderId, userId);
            if (reminder == null) return false;

            reminder.IsEnabled = enable;
            reminder.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task ProcessDueRemindersAsync(DateTime processTime)
        {
            // Get reminders that are due in the next minute
            var toTime = processTime.AddMinutes(1);
            var dueReminders = await _reminderRepository.GetDueRemindersAsync(processTime, toTime);

            foreach (var reminder in dueReminders)
            {
                try
                {
                    if (!reminder.IsEnabled) continue;

                    var notification = new ReminderNotificationDto
                    {
                        ReminderId = reminder.ReminderId,
                        UserId = reminder.UserId,
                        Title = reminder.Title,
                        Description = reminder.Description,
                        NotificationChannel = reminder.NotificationChannel,
                        TriggerTime = processTime
                    };

                    // TODO: Send notification when notification service is implemented
                    // await _notificationService.SendAsync(notification);

                    reminder.LastTriggered = processTime;
                    reminder.UpdatedAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing reminder {reminder.ReminderId}");
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}