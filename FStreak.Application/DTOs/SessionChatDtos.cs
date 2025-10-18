using FStreak.Domain.Entities;

namespace FStreak.Application.DTOs
{
    public class SessionMessageDto
    {
        public int MessageId { get; set; }
        public int SessionId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public MessageType Type { get; set; }
        public string? Text { get; set; }
        public string? GifUrl { get; set; }
        public DateTime Timestamp { get; set; }
        public List<SessionReactionDto> Reactions { get; set; } = new List<SessionReactionDto>();
    }

    public class SendMessageDto
    {
        public MessageType Type { get; set; }
        public string? Text { get; set; }
        public string? GifUrl { get; set; }
    }

    public class SessionReactionDto
    {
        public int ReactionId { get; set; }
        public int? MessageId { get; set; }
        public string? TargetUserId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Emoji { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class AddReactionDto
    {
        public int? MessageId { get; set; }
        public string? TargetUserId { get; set; }
        public string Emoji { get; set; } = string.Empty;
    }
}