using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public class SessionMessage : BaseEntity
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public int SessionId { get; set; }

        [ForeignKey(nameof(SessionId))]
        public virtual StudySession Session { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        public MessageType Type { get; set; }

        [MaxLength(1000)]
        public string? Text { get; set; }

        [MaxLength(500)]
        public string? GifUrl { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public virtual ICollection<SessionReaction> Reactions { get; set; } = new List<SessionReaction>();
    }
}