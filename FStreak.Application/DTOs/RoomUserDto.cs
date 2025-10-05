using System;
using FStreak.Domain.Entities;

namespace FStreak.Application.DTOs
{
    public class RoomUserDto
    {
        public int RoomUserId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
        public bool IsOnline { get; set; }
    }
}