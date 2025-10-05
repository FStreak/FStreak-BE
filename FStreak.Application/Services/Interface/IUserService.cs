using FStreak.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FStreak.Application.Services.Interface
{
    public interface IUserService
    {
        Task<IEnumerable<ApplicationUser>> GetAllAsync();
    }
}
