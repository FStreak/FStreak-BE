using System.ComponentModel.DataAnnotations;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public class GroupInvite : BaseEntity
    {
        [Key]
        public int GroupInviteId { get; set; }

        [Required]
        public int GroupId { get; set; }
        public virtual StudyGroup Group { get; set; } = null!;

        [Required]
        public string InvitedByUserId { get; set; } = string.Empty;
        public virtual ApplicationUser InvitedBy { get; set; } = null!;

        [Required]
        public string InvitedUserId { get; set; } = string.Empty;
        public virtual ApplicationUser InvitedUser { get; set; } = null!;

        public string? Message { get; set; }
        
        [Required]
        public InviteStatus Status { get; set; }

        public DateTime ExpiresAt { get; set; }
    }

    public enum InviteStatus
    {
        Pending,
        Accepted,
        Declined,
        Expired,
        Revoked
    }
}