using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public class UserChallenge : BaseEntity
    {
        [Key]
        public int UserChallengeId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ChallengeId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "IN_PROGRESS";

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("ChallengeId")]
        public virtual Challenge Challenge { get; set; }
    }
}