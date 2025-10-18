using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using FStreak.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FStreak.Infrastructure.Repositories
{
    public class PushSubscriptionRepository : Repository<PushSubscription>, IPushSubscriptionRepository
    {
        public PushSubscriptionRepository(FStreakDbContext context) : base(context)
        {
        }

        public async Task<List<PushSubscription>> GetActiveSubscriptionsForUserAsync(string userId)
        {
            return await _dbSet
                .Where(s => s.UserId == userId && s.Enabled)
                .ToListAsync();
        }

        public async Task<List<PushSubscription>> GetActiveSubscriptionsForUsersAsync(IEnumerable<string> userIds)
        {
            return await _dbSet
                .Where(s => userIds.Contains(s.UserId) && s.Enabled)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(string endpoint)
        {
            return await _dbSet.AnyAsync(s => s.Endpoint == endpoint);
        }

        public async Task DisableSubscriptionAsync(string endpoint)
        {
            var subscription = await _dbSet.FirstOrDefaultAsync(s => s.Endpoint == endpoint);
            if (subscription != null)
            {
                subscription.Enabled = false;
                subscription.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}