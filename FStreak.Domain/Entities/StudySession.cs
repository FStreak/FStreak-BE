using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public class StudySession : BaseEntity
    {
        [Key]
        public int StudySessionId { get; set; }

        [Required]
        public int StudyGroupId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int PomodoroCount { get; set; }

        // Navigation properties
        [ForeignKey("StudyGroupId")]
        public virtual StudyGroup StudyGroup { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; }
    }
}