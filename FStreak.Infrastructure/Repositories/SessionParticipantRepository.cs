using Microsoft.EntityFrameworkCore;
using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using FStreak.Infrastructure.Data;

namespace FStreak.Infrastructure.Repositories
{
    public class SessionParticipantRepository : Repository<SessionParticipant>
    {
        public SessionParticipantRepository(FStreakDbContext context) : base(context)
        {
        }

        public async Task<List<SessionParticipant>> GetSessionParticipantsAsync(int sessionId)
        {
            return await _dbSet
                .Include(sp => sp.User)
                .Where(sp => sp.SessionId == sessionId)
                .ToListAsync();
        }

        public async Task<SessionParticipant> GetParticipantAsync(int sessionId, string userId)
        {
            return await _dbSet
                .Include(sp => sp.User)
                .FirstOrDefaultAsync(sp => sp.SessionId == sessionId && sp.UserId == userId);
        }

        public async Task<bool> IsUserInSessionAsync(int sessionId, string userId)
        {
            return await _dbSet
                .AnyAsync(sp => sp.SessionId == sessionId && sp.UserId == userId);
        }
    }
}