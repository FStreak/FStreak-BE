using System.ComponentModel.DataAnnotations;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public class StudyGroup : BaseEntity
    {
        [Key]
        public int StudyGroupId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string InviteCode { get; set; } = string.Empty;

        [Required]
        public GroupVisibility Visibility { get; set; }

        [Required]
        public string OwnerId { get; set; } = string.Empty;

        public virtual ApplicationUser Owner { get; set; } = null!;
        
        public virtual ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
        public virtual ICollection<StudySession> Sessions { get; set; } = new List<StudySession>();
        public virtual ICollection<GroupInvite> Invites { get; set; } = new List<GroupInvite>();
    }

    public enum GroupVisibility
    {
        Private,
        Public
    }
}