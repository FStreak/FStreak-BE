using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FStreak.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }

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
    }
}