using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public class SessionParticipant : BaseEntity
    {
        [Key]
        public int ParticipantId { get; set; }

        [Required]
        public int SessionId { get; set; }

        [ForeignKey(nameof(SessionId))]
        public virtual StudySession Session { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        public ParticipantStatus Status { get; set; } = ParticipantStatus.Online;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LeftAt { get; set; }

        public int TotalFocusMinutes { get; set; }
        public int CompletedPomodoroRounds { get; set; }
    }

    public enum ParticipantStatus
    {
        Online,
        Typing,
        Focus,
        Away,
        Left
    }
}