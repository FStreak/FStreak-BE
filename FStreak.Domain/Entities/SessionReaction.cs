using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public class SessionReaction : BaseEntity
    {
        [Key]
        public int ReactionId { get; set; }

        [Required]
        public ReactionTargetType TargetType { get; set; }

        public int? MessageId { get; set; }

        [ForeignKey(nameof(MessageId))]
        public virtual SessionMessage? Message { get; set; }

        public string? TargetUserId { get; set; }

        [ForeignKey(nameof(TargetUserId))]
        public virtual ApplicationUser? TargetUser { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        [MaxLength(10)]
        public string Emoji { get; set; } = string.Empty;

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public enum ReactionTargetType
    {
        Message,
        User
    }
}