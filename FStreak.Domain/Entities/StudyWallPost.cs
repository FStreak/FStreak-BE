using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public class StudyWallPost : BaseEntity
    {
        [Key]
        public int StudyWallPostId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string ImageUrl { get; set; }

        [MaxLength(500)]
        public string Caption { get; set; }

        [Required]
        public int StudyMinutes { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}