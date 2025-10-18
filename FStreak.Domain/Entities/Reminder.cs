using System.ComponentModel.DataAnnotations;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public class Reminder : BaseEntity
    {
        [Key]
        public int ReminderId { get; set; }

        [Required]
        public required string UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public string TimeZoneId { get; set; } = "Asia/Ho_Chi_Minh";

        [Required]
        public TimeSpan TimeOfDay { get; set; }

        public string DaysOfWeek { get; set; } = "1,2,3,4,5"; // Comma-separated days (1=Monday, 7=Sunday)

        public string NotificationChannel { get; set; } = "email"; // email, push, etc.

        public bool IsEnabled { get; set; } = true;

        public DateTime? LastTriggered { get; set; }

        // Navigation properties
        public required virtual ApplicationUser User { get; set; }

        // Helper methods to handle days of week
        public List<DayOfWeek> GetDaysOfWeek()
        {
            return DaysOfWeek.Split(',')
                .Select(d => (DayOfWeek)((int.Parse(d) % 7)))
                .ToList();
        }

        public void SetDaysOfWeek(IEnumerable<DayOfWeek> days)
        {
            DaysOfWeek = string.Join(",", days.Select(d => ((int)d + 1).ToString()));
        }
    }
}