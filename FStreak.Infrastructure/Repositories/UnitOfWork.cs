using FStreak.Domain.Interfaces;
using FStreak.Domain.Entities;
using FStreak.Infrastructure.Data;

namespace FStreak.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FStreakDbContext _context;
        private bool disposed = false;

        // Thay thế User bằng ApplicationUser
        private IRepository<ApplicationUser> _users;
        private IRepository<StreakLog> _streakLogs;
        private IRepository<StudyGroup> _studyGroups;
        private IRepository<StudySession> _studySessions;
        private IRepository<Subject> _subjects;
        private IRepository<Challenge> _challenges;
        private IRepository<UserChallenge> _userChallenges;
        private IRepository<Badge> _badges;
        private IRepository<UserBadge> _userBadges;
        private IRepository<StudyWallPost> _studyWallPosts;
        private IRepository<UserFriend> _userFriends;
        private IRepository<Reaction> _reactions;
        private IRepository<StudyRoom> _studyRooms;
        private IRepository<RoomUser> _roomUsers;
        private IRepository<RoomMessage> _roomMessages;
        private IRepository<RefreshToken> _refreshTokens;

        public UnitOfWork(FStreakDbContext context)
        {
            _context = context;
        }

        // Thay đổi kiểu User thành ApplicationUser
        public IRepository<ApplicationUser> Users => _users ??= new Repository<ApplicationUser>(_context);
        public IRepository<StreakLog> StreakLogs => _streakLogs ??= new Repository<StreakLog>(_context);
        public IRepository<StudyGroup> StudyGroups => _studyGroups ??= new Repository<StudyGroup>(_context);
        public IRepository<StudySession> StudySessions => _studySessions ??= new Repository<StudySession>(_context);
        public IRepository<Subject> Subjects => _subjects ??= new Repository<Subject>(_context);
        public IRepository<Challenge> Challenges => _challenges ??= new Repository<Challenge>(_context);
        public IRepository<UserChallenge> UserChallenges => _userChallenges ??= new Repository<UserChallenge>(_context);
        public IRepository<Badge> Badges => _badges ??= new Repository<Badge>(_context);
        public IRepository<UserBadge> UserBadges => _userBadges ??= new Repository<UserBadge>(_context);
        public IRepository<StudyWallPost> StudyWallPosts => _studyWallPosts ??= new Repository<StudyWallPost>(_context);
        public IRepository<UserFriend> UserFriends => _userFriends ??= new Repository<UserFriend>(_context);
        public IRepository<Reaction> Reactions => _reactions ??= new Repository<Reaction>(_context);

        // Thêm các repository mới
        public IRepository<StudyRoom> StudyRooms => _studyRooms ??= new Repository<StudyRoom>(_context);
        public IRepository<RoomUser> RoomUsers => _roomUsers ??= new Repository<RoomUser>(_context);
        public IRepository<RoomMessage> RoomMessages => _roomMessages ??= new Repository<RoomMessage>(_context);
        public IRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new Repository<RefreshToken>(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}