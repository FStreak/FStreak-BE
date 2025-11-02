using FStreak.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FStreak.Application.Services.Interface
{
    public interface IFriendService
    {
        //lấy danh sách bạn bè của người dùng
        Task<Result<List<FriendDTO>>> GetFriendsAsync(string userId);

        //lấy danh sách lời mời kết bạn của người dùng
        Task<Result<FriendRequestsResponse>> GetFriendRequestsAsync(string userId);

        //gửi lời mời kết bạn
        Task<Result<FriendRequestDTO>> SendFriendRequestAsync(string senderId, string receiverId);

        //phản hồi lời mời kết bạn (chấp nhận hoặc từ chối)
        Task<Result<FriendDTO>> RespondToFriendRequestAsync(int requestId, string userId, bool accept);

        // hủy kết bạn
        Task<Result<bool>> UnfriendAsync(int friendshipId, string userId);

        // hủy lời mời kết bạn đã gửi
        Task<Result<bool>> CancelFriendRequestAsync(int requestId, string userId);

        // kiểm tra trạng thái kết bạn giữa hai người dùng
        Task<Result<bool>> CheckFriendshipStatusAsync(string userId, string targetUserId);
    }
}
