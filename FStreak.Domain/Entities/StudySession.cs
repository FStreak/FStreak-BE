using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public class StudySession : BaseEntity
    {
        [Key]
        public int SessionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int GroupId { get; set; }

        [ForeignKey(nameof(GroupId))]
        public virtual StudyGroup Group { get; set; } = null!;

        [Required]
        public SessionMode Mode { get; set; }

        public PomodoroConfig? PomodoroConfig { get; set; }

        [Required]
        public DateTime StartAt { get; set; }

        public int DurationMinutes { get; set; }

        [Required]
        public SessionStatus Status { get; set; } = SessionStatus.Scheduled;

        [Required]
        public string HostId { get; set; } = string.Empty;

        [ForeignKey(nameof(HostId))]
        public virtual ApplicationUser Host { get; set; } = null!;

        public virtual ICollection<SessionParticipant> Participants { get; set; } = new List<SessionParticipant>();
        public virtual ICollection<SessionMessage> Messages { get; set; } = new List<SessionMessage>();
        public virtual ICollection<SessionReaction> Reactions { get; set; } = new List<SessionReaction>();
    }

    public enum SessionMode
    {
        Free,
        Pomodoro
    }

    public enum SessionStatus
    {
        Scheduled,
        Live,
        Paused,
        Ended
    }

    public class PomodoroConfig
    {
        public int FocusMinutes { get; set; } = 25;
        public int BreakMinutes { get; set; } = 5;
        public int Rounds { get; set; } = 4;

        public DateTime? CurrentPhaseStartTime { get; set; }
        public PomodoroPhase CurrentPhase { get; set; }
        public int CurrentRound { get; set; } = 1;
    }

    public enum PomodoroPhase
    {
        Focus,
        Break
    }
}