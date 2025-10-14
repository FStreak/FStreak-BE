using FStreak.Domain.Common;

namespace FStreak.Domain.Entities;

public class StudyRoom : BaseEntity
{
    public int StudyRoomId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public string? InviteCode { get; set; } // vd: mkj-xbyr-hen

    //Bổ sung thêm các field
    public bool IsActive { get; set; } = true;
    public string? MeetingLink { get; set; } //Link tham gia phòng học
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }


    // Navigation properties
    public string CreatedById { get; set; } = string.Empty;
    public ApplicationUser CreatedBy { get; set; } = null!;
    public ICollection<RoomUser> RoomUsers { get; set; } = new List<RoomUser>();
    public ICollection<RoomMessage> Messages { get; set; } = new List<RoomMessage>();
}