using System.ComponentModel.DataAnnotations;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public class Lesson : BaseEntity
    {
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime? StartAt { get; set; }
        
        public int DurationMinutes { get; set; }
        
        public bool IsPublished { get; set; }

        [Required]
        public string CreatedById { get; set; } = string.Empty;

        // Navigation properties
        public virtual ApplicationUser CreatedBy { get; set; } = null!;
    }
}
