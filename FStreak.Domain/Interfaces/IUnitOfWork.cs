using FStreak.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace FStreak.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<ApplicationUser> Users { get; }
        IRepository<StreakLog> StreakLogs { get; }
        IRepository<StudyGroup> StudyGroups { get; }
        IRepository<StudySession> StudySessions { get; }
        IRepository<Subject> Subjects { get; }
        IRepository<Challenge> Challenges { get; }
        IRepository<UserChallenge> UserChallenges { get; }
        IRepository<Badge> Badges { get; }
        IRepository<UserBadge> UserBadges { get; }
        IRepository<StudyWallPost> StudyWallPosts { get; }
        IRepository<UserFriend> UserFriends { get; }
        IRepository<Reaction> Reactions { get; }

        // Thêm các repository mới
        IRepository<StudyRoom> StudyRooms { get; }
        IRepository<RoomUser> RoomUsers { get; }
        IRepository<RoomMessage> RoomMessages { get; }
        IRepository<RefreshToken> RefreshTokens { get; }
        
        // Group Study Repositories
        IRepository<GroupMember> GroupMembers { get; }
        IRepository<GroupInvite> GroupInvites { get; }
        IRepository<SessionParticipant> SessionParticipants { get; }
        IRepository<SessionMessage> SessionMessages { get; }
        IRepository<SessionReaction> SessionReactions { get; }
        
        // Lesson Repository
        ILessonRepository Lessons { get; }
        
        // Push Notification
        IPushSubscriptionRepository PushSubscriptions { get; }

        // Achievement Repositories
        IAchievementRepository Achievements { get; }
        IRepository<UserAchievement> UserAchievements { get; }

        // Shop Repositories
        IRepository<ShopItem> ShopItems { get; }
        IRepository<ShopOrder> ShopOrders { get; }
        IRepository<ShopOrderItem> ShopOrderItems { get; }
        IRepository<Payment> Payments { get; }

        Task<int> SaveChangesAsync();
    }
}