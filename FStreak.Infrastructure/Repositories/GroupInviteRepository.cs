using Microsoft.EntityFrameworkCore;
using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using FStreak.Infrastructure.Data;

namespace FStreak.Infrastructure.Repositories
{
    public class GroupInviteRepository : Repository<GroupInvite>
    {
        public GroupInviteRepository(FStreakDbContext context) : base(context)
        {
        }

        public async Task<List<GroupInvite>> GetPendingInvitesForUserAsync(string userId)
        {
            return await _dbSet
                .Include(gi => gi.Group)
                .Include(gi => gi.InvitedBy)
                .Where(gi => gi.InvitedUserId == userId && gi.Status == InviteStatus.Pending)
                .ToListAsync();
        }

        public async Task<GroupInvite> GetInviteByCodeAsync(string inviteCode)
        {
            return await _dbSet
                .Include(gi => gi.Group)
                .Include(gi => gi.InvitedBy)
                .Include(gi => gi.InvitedUser)
                .FirstOrDefaultAsync(gi => gi.Message == inviteCode);
        }
    }
}