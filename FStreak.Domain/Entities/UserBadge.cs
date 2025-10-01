using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public class UserBadge : BaseEntity
    {
        [Key]
        public int UserBadgeId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int BadgeId { get; set; }

        [Required]
        public DateTime EarnedDate { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("BadgeId")]
        public virtual Badge Badge { get; set; }
    }
}