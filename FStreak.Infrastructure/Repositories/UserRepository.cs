using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using FStreak.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FStreak.Infrastructure.Repositories
{
    public class UserRepository : Repository<ApplicationUser>, IUserRepository
    {
        public UserRepository(FStreakDbContext context) : base(context)
        {
        }
        //viết lại hàm khác vì userId là string, còn repository là int
        public async Task<ApplicationUser> GetByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }
    }
}
