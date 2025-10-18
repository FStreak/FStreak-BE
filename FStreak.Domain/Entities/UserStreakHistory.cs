using System.ComponentModel.DataAnnotations;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public class UserStreakHistory : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string UserId { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        public int StudyRoomId { get; set; }

        public int StreakCount { get; set; }

        // Navigation properties
        public required virtual ApplicationUser User { get; set; }
        public virtual StudyRoom? StudyRoom { get; set; }
    }
}