using System.ComponentModel.DataAnnotations;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public class GroupMember : BaseEntity
    {
        [Key]
        public int GroupMemberId { get; set; }

        [Required]
        public int GroupId { get; set; }
        public virtual StudyGroup Group { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        public GroupRole Role { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }

    public enum GroupRole
    {
        Member,
        Moderator,
        Admin
    }
}