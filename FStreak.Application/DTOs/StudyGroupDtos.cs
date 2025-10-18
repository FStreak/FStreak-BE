using FStreak.Domain.Entities;

namespace FStreak.Application.DTOs
{
    public class StudyGroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string InviteCode { get; set; } = string.Empty;
        public GroupVisibility Visibility { get; set; }
        public string OwnerId { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateStudyGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public GroupVisibility Visibility { get; set; }
    }

    public class UpdateStudyGroupDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public GroupVisibility? Visibility { get; set; }
    }

    public class CreateGroupInviteDto
    {
        public string InvitedUserId { get; set; } = string.Empty;
    }

    public class GroupInviteDto
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string InvitedByUserId { get; set; } = string.Empty;
        public string InvitedByUserName { get; set; } = string.Empty;
        public string InvitedUserId { get; set; } = string.Empty;
        public string InvitedUserName { get; set; } = string.Empty;
        public InviteStatus Status { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}