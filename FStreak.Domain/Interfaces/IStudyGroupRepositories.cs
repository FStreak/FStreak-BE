using FStreak.Domain.Entities;

namespace FStreak.Domain.Interfaces
{
    public interface IStudyGroupRepository : IRepository<StudyGroup>
    {
        Task<StudyGroup?> GetByInviteCodeAsync(string code);
        Task<bool> IsUserMemberAsync(int groupId, string userId);
        Task<GroupRole?> GetUserRoleAsync(int groupId, string userId);
        Task<bool> HasPermissionAsync(int groupId, string userId, GroupRole minimumRole);
        Task<List<StudyGroup>> GetUserGroupsAsync(string userId, int skip = 0, int take = 20);
        Task<List<GroupMember>> GetGroupMembersAsync(int groupId, int skip = 0, int take = 50);
        Task<int> GetMemberCountAsync(int groupId);
    }

    public interface IStudySessionRepository : IRepository<StudySession>
    {
        Task<List<StudySession>> GetActiveSessionsAsync(int? groupId = null);
        Task<List<StudySession>> GetGroupSessionsAsync(int groupId, bool includeEnded = false, int skip = 0, int take = 20);
        Task<List<StudySession>> GetUserSessionsAsync(string userId, bool includeEnded = false, int skip = 0, int take = 20);
        Task<List<SessionParticipant>> GetSessionParticipantsAsync(int sessionId);
        Task<bool> IsUserParticipantAsync(int sessionId, string userId);
    }

    public interface ISessionMessageRepository : IRepository<SessionMessage>
    {
        Task<List<SessionMessage>> GetSessionMessagesAsync(int sessionId, int? beforeMessageId = null, int take = 50);
        Task<int> GetUnreadCountAsync(int sessionId, string userId, DateTime? lastReadAt = null);
    }

    public interface ISessionReactionRepository : IRepository<SessionReaction>
    {
        Task<List<SessionReaction>> GetMessageReactionsAsync(int messageId);
        Task<List<SessionReaction>> GetUserReactionsAsync(string targetUserId);
    }
}