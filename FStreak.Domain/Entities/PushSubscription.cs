using System.ComponentModel.DataAnnotations;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public class PushSubscription : BaseEntity
    {
        [Key]
        public int PushSubscriptionId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Endpoint { get; set; } = string.Empty;

        [Required]
        public string P256dh { get; set; } = string.Empty;

        [Required]
        public string Auth { get; set; } = string.Empty;

        public string? UserAgent { get; set; }

        public bool Enabled { get; set; } = true;

        public DateTime? LastUsed { get; set; }

        // Navigation property
        public virtual ApplicationUser User { get; set; } = null!;
    }
}