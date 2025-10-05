namespace FStreak.Application.Services
{
    /// <summary>
    /// DTO for temporarily storing study tracking info in cache.
    /// </summary>
    public class StudyTrackingInfo
    {
        public int RoomId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
    }
}
