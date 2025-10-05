using FStreak.Domain.Common;

namespace FStreak.Domain.Entities;

public class RoomUser : BaseEntity
{
    public int RoomUserId { get; set; }
    public int StudyRoomId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public bool IsOnline { get; set; }
    public TimeSpan TotalStudyTime { get; set; }
    public bool StreakEarned { get; set; }
    
    // Navigation properties
    public StudyRoom Room { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}