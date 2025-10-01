using FStreak.Domain.Interfaces;
using FStreak.Domain.Entities;
using FStreak.Infrastructure.Data;

namespace FStreak.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FStreakDbContext _context;
        private bool disposed = false;

        private IRepository<User> _users;
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

        public UnitOfWork(FStreakDbContext context)
        {
            _context = context;
        }

        public IRepository<User> Users => _users ??= new Repository<User>(_context);
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