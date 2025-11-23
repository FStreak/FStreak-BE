using FStreak.Application.Services.Interface;
using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FStreak.Application.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepo;
        public UserService(IUnitOfWork unitOfWork, IUserRepository userRepo)
        {
            _unitOfWork = unitOfWork;
            _userRepo = userRepo;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
        {
             return await _unitOfWork.Users.GetAllAsync();
        }

        public async Task<ApplicationUser> GetByUserIdAsync(string id)
        {
            return await _userRepo.GetByIdAsync(id);
        }
    }
}
