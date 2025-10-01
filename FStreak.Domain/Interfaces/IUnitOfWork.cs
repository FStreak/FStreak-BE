using FStreak.Domain.Entities;

namespace FStreak.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
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

        Task<int> SaveChangesAsync();
    }
}