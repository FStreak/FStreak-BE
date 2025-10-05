using System;
using FStreak.Domain.Entities;

namespace FStreak.Application.DTOs
{
    public class RoomMessageDto
    {
        public int RoomMessageId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public MessageType Type { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}