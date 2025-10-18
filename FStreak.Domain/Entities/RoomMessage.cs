using FStreak.Domain.Common;

namespace FStreak.Domain.Entities;

public class RoomMessage : BaseEntity
{
    public int RoomMessageId { get; set; }
    public int StudyRoomId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    
    // Navigation properties
    public StudyRoom Room { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}