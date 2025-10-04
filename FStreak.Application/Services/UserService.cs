using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FStreak.Application.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
    }
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _unitOfWork.Users.GetAllAsync();
        }
    }
}
