using Microsoft.EntityFrameworkCore;
using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using FStreak.Infrastructure.Data;

namespace FStreak.Infrastructure.Repositories
{
    public class SessionMessageRepository : Repository<SessionMessage>
    {
        public SessionMessageRepository(FStreakDbContext context) : base(context)
        {
        }

        public async Task<List<SessionMessage>> GetSessionMessagesAsync(int sessionId, int skip = 0, int take = 50)
        {
            return await _dbSet
                .Include(sm => sm.User)
                .Include(sm => sm.Reactions)
                .Where(sm => sm.SessionId == sessionId)
                .OrderByDescending(sm => sm.Timestamp)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<SessionMessage> GetMessageWithReactionsAsync(int messageId)
        {
            return await _dbSet
                .Include(sm => sm.User)
                .Include(sm => sm.Reactions)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(sm => sm.MessageId == messageId);
        }
    }
}