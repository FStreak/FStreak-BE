using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using FStreak.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FStreak.Infrastructure.Repositories
{
    public class StudyRoomRepository : Repository<StudyRoom>, IStudyRoomRepository
    {
        public StudyRoomRepository(FStreakDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<StudyRoom>> GetActiveRoomsAsync()
        {
            return await _context.StudyRooms
                .Include(r => r.RoomUsers)
                .Include(r => r.Messages)
                .Where(r => r.IsActive && r.EndTime > DateTime.UtcNow) // Lát nữa sửa lại theo múi giờ VN
                .ToListAsync();
        }

        //roomCode này chính là InviteCode trong entity StudyRoom
        public async Task<StudyRoom> GetByRoomCodeAsync(string roomCode)
        {
            return await _context.StudyRooms
                .Include(r => r.RoomUsers)
                .Include(r => r.Messages)
                .FirstOrDefaultAsync(r => r.InviteCode == roomCode);
        }

        public async Task<IEnumerable<StudyRoom>> GetRoomByUserIdAsync(string userId)
        {
            return await _context.StudyRooms
                .Include(r => r.RoomUsers)
                .Include(r=> r.Messages)
                .Where(r=>r.RoomUsers.Any(ru => ru.UserId == userId))
                .ToListAsync();
        }
    }
}
