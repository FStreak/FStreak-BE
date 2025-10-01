using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FStreak.Domain.Common;

namespace FStreak.Domain.Entities
{
    public class User : BaseEntity
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        [Required]
        [MaxLength(255)]
        public string Password { get; set; }

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
    }
}