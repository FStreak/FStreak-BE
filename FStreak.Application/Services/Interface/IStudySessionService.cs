using FStreak.Application.DTOs;
using FStreak.Domain.Entities;

namespace FStreak.Application.Services.Interface
{
    public interface IStudySessionService
    {
        // Session Management
        Task<Result<StudySessionDto>> CreateSessionAsync(string userId, CreateStudySessionDto dto);
        Task<Result<StudySessionDto>> GetSessionByIdAsync(int sessionId);
        Task<Result<List<StudySessionDto>>> GetActiveGroupSessionsAsync(int groupId);
        Task<Result<StudySessionDto>> StartSessionAsync(int sessionId, string hostId);
        Task<Result<StudySessionDto>> EndSessionAsync(int sessionId, string hostId);
        Task<Result<bool>> DeleteSessionAsync(int sessionId, string hostId);

        // Participant Management
        Task<Result<SessionParticipantDto>> JoinSessionAsync(int sessionId, string userId);
        Task<Result<bool>> LeaveSessionAsync(int sessionId, string userId);
        Task<Result<bool>> UpdateParticipantStatusAsync(int sessionId, string userId, bool isFocused);
        Task<Result<List<SessionParticipantDto>>> GetSessionParticipantsAsync(int sessionId);

        // Pomodoro Timer Management
        Task<Result<StudySessionDto>> StartPomodoroPhaseAsync(int sessionId, string hostId);
        Task<Result<StudySessionDto>> EndPomodoroPhaseAsync(int sessionId, string hostId);
        Task<Result<StudySessionDto>> SkipPomodoroPhaseAsync(int sessionId, string hostId);
        Task<Result<StudySessionDto>> UpdatePomodoroSettingsAsync(int sessionId, string hostId, PomodoroConfig settings);

        // Chat & Reactions
        Task<Result<SessionMessageDto>> SendMessageAsync(int sessionId, string userId, SendMessageDto dto);
        Task<Result<List<SessionMessageDto>>> GetSessionMessagesAsync(int sessionId, int skip = 0, int take = 50);
        Task<Result<SessionReactionDto>> AddReactionAsync(int sessionId, string userId, AddReactionDto dto);
        Task<Result<bool>> RemoveReactionAsync(int sessionId, string userId, int reactionId);
    }
}