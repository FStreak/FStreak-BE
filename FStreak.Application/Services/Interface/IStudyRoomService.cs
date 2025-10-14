using FStreak.Application.DTOs;
using FStreak.Domain.Entities;

namespace FStreak.Application.Services.Interface
{
    public interface IStudyRoomService
    {
        //User vào phòng học
        Task<Result<RoomUserDto>> JoinRoom(int roomId, string userId);
        //User rời phòng học
        Task<Result<RoomUserDto>> LeaveRoom(int roomId, string userId);
        //User gửi tin nhắn trong phòng học
        Task<Result<RoomMessageDto>> AddMessage(int roomId, string userId, string content, MessageType type);
        //Xử lý kết nối của user
        Task HandleConnection(string userId);
        //Xử lý ngắt kết nối của user
        Task HandleDisconnection(string userId);

        //User tham gia phòng học bằng mã mời(InviteCode)
        Task<Result<RoomUserDto>> JoinRoomByCodeAsync(string roomCode, string userId);


        //Khởi tạo phòng học mới
        Task<Result<StudyRoomDto>> CreateRoomAsync(CreateRoomDto createDto, string userId);

        //Lấy thông tin phòng học theo mã mời(InviteCode)
        Task<Result<StudyRoomDto>> GetRoomByCodeAsync(string roomCode);
        //Lấy phòng học theo Id
        Task<Result<StudyRoomDto>> GetRoomByIdAsync(int roomId);

        //Lấy danh sách phòng học đang hoạt động
        Task<Result<List<StudyRoomDto>>> GetActiveRoomsAsync();
        //Kết thúc phòng học, chỉ chủ phòng mới có quyền
        Task<Result<bool>> EndRoomAsync(int roomId, string userId);
    }
}