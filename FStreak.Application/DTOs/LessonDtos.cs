using System.ComponentModel.DataAnnotations;

namespace FStreak.Application.DTOs
{
    public class LessonCreateDto
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
    }

    public class LessonUpdateDto
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
    }

    public class LessonReadDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartAt { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsPublished { get; set; }
        public string CreatedById { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
