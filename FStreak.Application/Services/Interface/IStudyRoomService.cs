using FStreak.Application.DTOs;
using FStreak.Domain.Entities;

namespace FStreak.Application.Services.Interface
{
    public interface IStudyRoomService
    {
        Task<Result<RoomUserDto>> JoinRoom(int roomId, string userId);
        Task<Result<RoomUserDto>> LeaveRoom(int roomId, string userId);
        Task<Result<RoomMessageDto>> AddMessage(int roomId, string userId, string content, MessageType type);
        Task HandleConnection(string userId);
        Task HandleDisconnection(string userId);
    }
}