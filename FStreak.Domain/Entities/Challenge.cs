using System.ComponentModel.DataAnnotations;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public class Challenge : BaseEntity
    {
        [Key]
        public int ChallengeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public int DurationDays { get; set; }

        [Required]
        public int TargetMinutes { get; set; }
    }
}