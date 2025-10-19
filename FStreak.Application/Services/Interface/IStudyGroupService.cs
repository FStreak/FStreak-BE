using FStreak.Application.DTOs;

namespace FStreak.Application.Services.Interface
{
    public interface IStudyGroupService
    {
        // Group Management
        Task<Result<StudyGroupDto>> CreateGroupAsync(string userId, CreateStudyGroupDto dto);
        Task<Result<StudyGroupDto>> UpdateGroupAsync(int groupId, string userId, UpdateStudyGroupDto dto);
        Task<Result<bool>> DeleteGroupAsync(int groupId, string userId);
        Task<Result<StudyGroupDto>> GetGroupByIdAsync(int groupId);
        Task<Result<List<StudyGroupDto>>> GetUserGroupsAsync(string userId);
        Task<Result<List<StudyGroupDto>>> SearchGroupsAsync(string searchTerm);

        // Member Management
        Task<Result<GroupMemberDto>> AddMemberAsync(int groupId, string userId, bool isAdmin = false);
        Task<Result<bool>> RemoveMemberAsync(int groupId, string adminId, string userId);
        Task<Result<bool>> LeaveGroupAsync(int groupId, string userId);
        Task<Result<List<GroupMemberDto>>> GetGroupMembersAsync(int groupId);
        Task<Result<bool>> UpdateMemberRoleAsync(int groupId, string adminId, string userId, bool isAdmin);

        // Invite Management
        Task<Result<GroupInviteDto>> CreateInviteAsync(int groupId, string inviterId, CreateGroupInviteDto dto);
        Task<Result<GroupInviteDto>> GetInviteByIdAsync(int inviteId);
        Task<Result<List<GroupInviteDto>>> GetPendingInvitesForUserAsync(string userId);
        Task<Result<GroupInviteDto>> RespondToInviteAsync(int inviteId, string userId, bool accept);
        Task<Result<bool>> CancelInviteAsync(int inviteId, string inviterId);
    }
}