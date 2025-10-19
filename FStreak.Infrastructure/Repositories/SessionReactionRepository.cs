using Microsoft.EntityFrameworkCore;
using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using FStreak.Infrastructure.Data;

namespace FStreak.Infrastructure.Repositories
{
    public class SessionReactionRepository : Repository<SessionReaction>
    {
        public SessionReactionRepository(FStreakDbContext context) : base(context)
        {
        }

        public async Task<List<SessionReaction>> GetMessageReactionsAsync(int messageId)
        {
            return await _dbSet
                .Include(sr => sr.User)
                .Where(sr => sr.MessageId == messageId)
                .ToListAsync();
        }

        public async Task<bool> HasUserReactedAsync(int messageId, string userId, string reactionType)
        {
            return await _dbSet
                .AnyAsync(sr => 
                    sr.MessageId == messageId && 
                    sr.UserId == userId && 
                    sr.Emoji == reactionType);
        }

        public async Task<SessionReaction> GetUserReactionAsync(int messageId, string userId, string reactionType)
        {
            return await _dbSet
                .Include(sr => sr.User)
                .FirstOrDefaultAsync(sr => 
                    sr.MessageId == messageId && 
                    sr.UserId == userId && 
                    sr.Emoji == reactionType);
        }
    }
}