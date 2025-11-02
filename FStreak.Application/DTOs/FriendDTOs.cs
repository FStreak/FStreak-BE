using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FStreak.Application.DTOs
{
    public class FriendDTO
    {
        public int FriendshipId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? Email { get; set; }
        public DateTime FriendsSince { get; set; }
        public bool IsOnline { get; set; }

    }

    public class FriendRequestDTO
    {
        public int RequestId { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string? SenderAvatarUrl { get; set; }
        public string ReceiverId { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string? ReceiverAvatarUrl { get; set; }
        public DateTime RequestedAt { get; set; }
        public string Status { get; set; } = "Pending"; // e.g., "Pending", "Accepted", "Rejected"
    }

    public class SendFriendRequestDTO
    {
        public string ReceiverId { get; set; } = string.Empty;
    }

    public class RespondFriendRequestDTO
    {
        public int RequestId { get; set; }
        public bool Accept { get; set; } // true to accept, false to reject
    }

    public class FriendRequestsResponse
    {
        public List<FriendRequestDTO> SentRequests { get; set; } = new();
        public List<FriendRequestDTO> ReceivedRequests { get; set; } = new();
    }
}
