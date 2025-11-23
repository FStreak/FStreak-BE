using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public class UserAchievement : BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public Guid AchievementId { get; set; }

        [Required]
        public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Progress { get; set; }

        public bool IsClaimed { get; set; } = false;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("AchievementId")]
        public virtual Achievement Achievement { get; set; } = null!;
    }
}

