using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FStreak.Application.DTOs
{
    public class LessonUpdateWithFileDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime? StartAt { get; set; }

        [Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1440 minutes")]
        public int DurationMinutes { get; set; }

        public bool IsPublished { get; set; }

        public IFormFile? DocumentFile { get; set; }
        public IFormFile? VideoFile { get; set; }
    }
}