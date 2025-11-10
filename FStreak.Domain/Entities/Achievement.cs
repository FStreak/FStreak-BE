using System.ComponentModel.DataAnnotations;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public class Achievement : BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int Points { get; set; }

        [MaxLength(2000)]
        public string? CriteriaJson { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<UserAchievement> UserAchievements { get; set; } = new HashSet<UserAchievement>();
    }
}

