using FStreak.Domain.Entities;

namespace FStreak.Application.DTOs
{
    public class StudySessionDto
    {
        public int SessionId { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string HostId { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public SessionMode Mode { get; set; }
        public SessionStatus Status { get; set; }
        public PomodoroConfig? PomodoroSettings { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int ParticipantCount { get; set; }
        public List<SessionParticipantDto> Participants { get; set; } = new List<SessionParticipantDto>();
        public DateTime CreatedAt { get; set; }
    }

    public class CreateStudySessionDto
    {
        public int GroupId { get; set; }
        public string Topic { get; set; } = string.Empty;
        public SessionMode Mode { get; set; }
        public PomodoroConfig? PomodoroSettings { get; set; }
    }

    public class SessionParticipantDto
    {
        public int ParticipantId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool IsHost { get; set; }
        public bool IsFocused { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime? LastActive { get; set; }
    }
}