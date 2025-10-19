using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FStreak.Application.DTOs
{
    public class PushSubscriptionDto
    {
        public int Id { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        public bool Enabled { get; set; }
        public DateTime? LastUsed { get; set; }
    }

    public class RegisterPushDeviceDto
    {
        [Required]
        public string Endpoint { get; set; } = string.Empty;

        [Required]
        public Dictionary<string, string> Keys { get; set; } = new Dictionary<string, string>();
    }

    public class PushNotificationDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Body { get; set; }

        public string? Icon { get; set; }

        public string? Badge { get; set; }

        public string? Tag { get; set; }

        public string? Image { get; set; }

        public Dictionary<string, string>? Data { get; set; }

        public int? TimeToLive { get; set; }

        public bool? RequireInteraction { get; set; }

        public List<PushActionDto>? Actions { get; set; }
    }

    public class PushActionDto
    {
        [Required]
        public string Action { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Icon { get; set; }
    }
}