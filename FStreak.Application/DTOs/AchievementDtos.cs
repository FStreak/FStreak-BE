namespace FStreak.Application.DTOs
{
    public class AchievementDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Points { get; set; }
        public string? CriteriaJson { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UserAchievementDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Guid AchievementId { get; set; }
        public AchievementDto? Achievement { get; set; }
        public DateTime EarnedAt { get; set; }
        public string? Progress { get; set; }
        public bool IsClaimed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateAchievementDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Points { get; set; }
        public string? CriteriaJson { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateAchievementDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? Points { get; set; }
        public string? CriteriaJson { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ClaimAchievementRequest
    {
        public Guid AchievementId { get; set; }
    }
}

