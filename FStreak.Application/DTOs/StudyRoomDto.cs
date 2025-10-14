using FStreak.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FStreak.Application.DTOs
{
    public class StudyRoomDto
    {
        public int StudyRoomId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsPrivate { get; set; }
        public string? InviteCode { get; set; } // vd: mkj-xbyr-hen

        public bool IsActive { get; set; } = true;
        public string? MeetingLink { get; set; } //Link tham gia phòng học
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string CreatedById { get; set; } = string.Empty;
        public UserDto CreatedBy { get; set; } = null!;
        public ICollection<RoomUserDto> RoomUsers { get; set; } = new List<RoomUserDto>();

    }

    public class CreateRoomDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsPrivate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
