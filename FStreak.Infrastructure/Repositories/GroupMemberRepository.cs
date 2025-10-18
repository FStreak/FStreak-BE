using Microsoft.EntityFrameworkCore;
using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using FStreak.Infrastructure.Data;

namespace FStreak.Infrastructure.Repositories
{
    public class GroupMemberRepository : Repository<GroupMember>
    {
        public GroupMemberRepository(FStreakDbContext context) : base(context)
        {
        }

        public async Task<List<GroupMember>> GetUserGroupMembershipsAsync(string userId)
        {
            return await _dbSet
                .Include(gm => gm.Group)
                .Include(gm => gm.User)
                .Where(gm => gm.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<GroupMember>> GetGroupMembersAsync(int groupId)
        {
            return await _dbSet
                .Include(gm => gm.User)
                .Where(gm => gm.GroupId == groupId)
                .ToListAsync();
        }
    }
}