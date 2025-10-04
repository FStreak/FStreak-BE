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
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(FStreakDbContext context) : base(context)
        {
        }
    }
}
