using FStreak.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FStreak.Domain.Interfaces
{
    public interface IStudyRoomRepository
    {
        //roomCode này chính là InviteCode trong entity StudyRoom
        Task<StudyRoom> GetByRoomCodeAsync(string roomCode);
        Task<IEnumerable<StudyRoom>> GetActiveRoomsAsync();
        Task<IEnumerable<StudyRoom>> GetRoomByUserIdAsync(string userId);
    }
}
