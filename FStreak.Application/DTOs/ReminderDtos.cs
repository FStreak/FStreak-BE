using System.ComponentModel.DataAnnotations;

namespace FStreak.Application.DTOs
{
    public class ReminderDto
    {
        public int ReminderId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string TimeZoneId { get; set; }
        public TimeSpan TimeOfDay { get; set; }
        public List<int> DaysOfWeek { get; set; } // 1 = Monday, 7 = Sunday
        public string NotificationChannel { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? LastTriggered { get; set; }
    }

    public class CreateReminderDto
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public string TimeZoneId { get; set; } = "Asia/Ho_Chi_Minh";

        [Required]
        public TimeSpan TimeOfDay { get; set; }

        [Required]
        [MinLength(1)]
        public List<int> DaysOfWeek { get; set; } = new() { 1, 2, 3, 4, 5 };

        public string NotificationChannel { get; set; } = "email";
    }

    public class UpdateReminderDto
    {
        [MaxLength(100)]
        public string? Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public string? TimeZoneId { get; set; }

        public TimeSpan? TimeOfDay { get; set; }

        public List<int>? DaysOfWeek { get; set; }

        public string? NotificationChannel { get; set; }

        public bool? IsEnabled { get; set; }
    }

    public class ReminderNotificationDto
    {
        public int ReminderId { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string NotificationChannel { get; set; }
        public DateTime TriggerTime { get; set; }
    }
}