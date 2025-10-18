using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FStreak.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            StreakLogs = new HashSet<StreakLog>();
            StudySessions = new HashSet<StudySession>();
            UserChallenges = new HashSet<UserChallenge>();
            UserBadges = new HashSet<UserBadge>();
            StudyWallPosts = new HashSet<StudyWallPost>();
            Friends = new HashSet<UserFriend>();
            FriendOf = new HashSet<UserFriend>();
            SentReactions = new HashSet<Reaction>();
            ReceivedReactions = new HashSet<Reaction>();
            RefreshTokens = new HashSet<RefreshToken>();
            GroupMemberships = new HashSet<GroupMember>();
            SentGroupInvites = new HashSet<GroupInvite>();
            ReceivedGroupInvites = new HashSet<GroupInvite>();
            SessionParticipations = new HashSet<SessionParticipant>();
            SessionMessages = new HashSet<SessionMessage>();
            SessionReactions = new HashSet<SessionReaction>();
        }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        
        [MaxLength(100)]
        public string? TimeZone { get; set; }

        // Navigation properties
        public virtual ICollection<StreakLog> StreakLogs { get; set; }
        public virtual ICollection<StudySession> StudySessions { get; set; }
        public virtual ICollection<UserChallenge> UserChallenges { get; set; }
        public virtual ICollection<UserBadge> UserBadges { get; set; }
        public virtual ICollection<StudyWallPost> StudyWallPosts { get; set; }
        public virtual ICollection<UserFriend> Friends { get; set; }
        public virtual ICollection<UserFriend> FriendOf { get; set; }
        public virtual ICollection<Reaction> SentReactions { get; set; }
        public virtual ICollection<Reaction> ReceivedReactions { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }

        // Group Study navigation properties
        public virtual ICollection<GroupMember> GroupMemberships { get; set; }
        public virtual ICollection<GroupInvite> SentGroupInvites { get; set; }
        public virtual ICollection<GroupInvite> ReceivedGroupInvites { get; set; }
        public virtual ICollection<SessionParticipant> SessionParticipations { get; set; }
        public virtual ICollection<SessionMessage> SessionMessages { get; set; }
        public virtual ICollection<SessionReaction> SessionReactions { get; set; }
    }
}