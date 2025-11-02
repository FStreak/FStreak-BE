using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FStreak.Application.DTOs;
using FStreak.Application.Services.Interface;
using FStreak.Domain.Common;
using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using FStreak.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FStreak.Application.Services.Implementation
{
    public class FriendService : IFriendService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userReop;
        private readonly ILogger<FriendService> _logger;

        public FriendService(IUnitOfWork unitOfWork, IUserRepository userRepo, ILogger<FriendService> logger)
        {
            _unitOfWork = unitOfWork;
            _userReop = userRepo;
            _logger = logger;
        }

        public async Task<Result<List<FriendDTO>>> GetFriendsAsync(string userId)
        {
            try
            {
                var friends = await _unitOfWork.UserFriends
                    .GetQueryable()
                    .Where(uf => (uf.UserId == userId || uf.FriendId == userId) && uf.Status == "Accepted")
                    .Select(uf => uf.UserId == userId ? uf.Friend : uf.User)
                    .Select(u => new FriendDTO
                    {
                        UserId = u.Id,
                        UserName = u.UserName ?? "Unknown",
                        AvatarUrl = u.AvatarUrl,
                        Email = u.Email,
                        FriendsSince = DateTime.UtcNow, // You may want to store this in UserFriend entity
                        IsOnline = false // Implement online status logic
                    })
                    .ToListAsync();

                return Result<List<FriendDTO>>.Success(friends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting friends for user {UserId}", userId);
                return Result<List<FriendDTO>>.Failure("Failed to get friends");
            }
        }

        public async Task<Result<FriendRequestsResponse>> GetFriendRequestsAsync(string userId)
        {
            try
            {
                var sentRequests = await _unitOfWork.UserFriends
                    .GetQueryable()
                    .Where(uf => uf.UserId == userId && uf.Status == "Pending")
                    .Include(uf => uf.Friend)
                    .Select(uf => new FriendRequestDTO
                    {
                        RequestId = uf.UserFriendId,
                        SenderId = uf.UserId,
                        SenderName = uf.User.UserName ?? "Unknown",
                        SenderAvatarUrl = uf.User.AvatarUrl,
                        ReceiverId = uf.FriendId,
                        ReceiverName = uf.Friend.UserName ?? "Unknown",
                        ReceiverAvatarUrl = uf.Friend.AvatarUrl,
                        RequestedAt = uf.CreatedAt,
                        Status = uf.Status
                    })
                    .ToListAsync();

                var receivedRequests = await _unitOfWork.UserFriends
                    .GetQueryable()
                    .Where(uf => uf.FriendId == userId && uf.Status == "Pending")
                    .Include(uf => uf.User)
                    .Select(uf => new FriendRequestDTO
                    {
                        RequestId = uf.UserFriendId,
                        SenderId = uf.UserId,
                        SenderName = uf.User.UserName ?? "Unknown",
                        SenderAvatarUrl = uf.User.AvatarUrl,
                        ReceiverId = uf.FriendId,
                        ReceiverName = uf.Friend.UserName ?? "Unknown",
                        ReceiverAvatarUrl = uf.Friend.AvatarUrl,
                        RequestedAt = uf.CreatedAt,
                        Status = uf.Status
                    })
                    .ToListAsync();

                var response = new FriendRequestsResponse
                {
                    SentRequests = sentRequests,
                    ReceivedRequests = receivedRequests
                };

                return Result<FriendRequestsResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting friend requests for user {UserId}", userId);
                return Result<FriendRequestsResponse>.Failure("Failed to get friend requests");
            }
        }

        public async Task<Result<FriendRequestDTO>> SendFriendRequestAsync(string senderId, string receiverId)
        {
            try
            {
                if (senderId == receiverId)
                    return Result<FriendRequestDTO>.Failure("Cannot send friend request to yourself");

                // Check if request already exists
                var existingRequest = await _unitOfWork.UserFriends
                    .GetQueryable()
                    .AnyAsync(uf => 
                        (uf.UserId == senderId && uf.FriendId == receiverId) ||
                        (uf.UserId == receiverId && uf.FriendId == senderId));

                if (existingRequest)
                    return Result<FriendRequestDTO>.Failure("Friend request already exists");

                // Check if receiver exists
                var receiver = await _userReop.GetByIdAsync(receiverId);
                if (receiver == null)
                    return Result<FriendRequestDTO>.Failure("User not found");

                var friendRequest = new UserFriend
                {
                    UserId = senderId,
                    FriendId = receiverId,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.UserFriends.AddAsync(friendRequest);
                await _unitOfWork.SaveChangesAsync();

                var sender = await _userReop.GetByIdAsync(senderId);

                var dto = new FriendRequestDTO
                {
                    RequestId = friendRequest.UserFriendId,
                    SenderId = senderId,
                    SenderName = sender?.UserName ?? "Unknown",
                    SenderAvatarUrl = sender?.AvatarUrl,
                    ReceiverId = receiverId,
                    ReceiverName = receiver.UserName ?? "Unknown",
                    ReceiverAvatarUrl = receiver.AvatarUrl,
                    RequestedAt = friendRequest.CreatedAt,
                    Status = friendRequest.Status
                };

                return Result<FriendRequestDTO>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending friend request from {SenderId} to {ReceiverId}", senderId, receiverId);
                return Result<FriendRequestDTO>.Failure("Failed to send friend request");
            }
        }

        public async Task<Result<FriendDTO>> RespondToFriendRequestAsync(int requestId, string userId, bool accept)
        {
            try
            {
                var request = await _unitOfWork.UserFriends
                    .GetQueryable()
                    .Include(uf => uf.User)
                    .Include(uf => uf.Friend)
                    .FirstOrDefaultAsync(uf => uf.UserFriendId == requestId);

                if (request == null)
                    return Result<FriendDTO>.Failure("Friend request not found");

                if (request.FriendId != userId)
                    return Result<FriendDTO>.Failure("You are not authorized to respond to this request");

                if (request.Status != "Pending")
                    return Result<FriendDTO>.Failure("This request has already been processed");

                if (accept)
                {
                    request.Status = "Accepted";
                    await _unitOfWork.SaveChangesAsync();

                    var friendDto = new FriendDTO
                    {
                        FriendshipId = request.UserFriendId,
                        UserId = request.UserId,
                        UserName = request.User.UserName ?? "Unknown",
                        AvatarUrl = request.User.AvatarUrl,
                        Email = request.User.Email,
                        FriendsSince = DateTime.UtcNow,
                        IsOnline = false
                    };

                    return Result<FriendDTO>.Success(friendDto);
                }
                else
                {
                    request.Status = "Rejected";
                    await _unitOfWork.SaveChangesAsync();
                    return Result<FriendDTO>.Failure("Friend request rejected");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error responding to friend request {RequestId}", requestId);
                return Result<FriendDTO>.Failure("Failed to respond to friend request");
            }
        }

        public async Task<Result<bool>> UnfriendAsync(int friendshipId, string userId)
        {
            try
            {
                var friendship = await _unitOfWork.UserFriends
                    .GetQueryable()
                    .FirstOrDefaultAsync(uf => 
                        uf.UserFriendId == friendshipId && 
                        (uf.UserId == userId || uf.FriendId == userId));

                if (friendship == null)
                    return Result<bool>.Failure("Friendship not found");

                await _unitOfWork.UserFriends.DeleteAsync(friendship);
                await _unitOfWork.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unfriending {FriendshipId}", friendshipId);
                return Result<bool>.Failure("Failed to unfriend");
            }
        }

        public async Task<Result<bool>> CancelFriendRequestAsync(int requestId, string userId)
        {
            try
            {
                var request = await _unitOfWork.UserFriends
                    .GetQueryable()
                    .FirstOrDefaultAsync(uf => uf.UserFriendId == requestId && uf.UserId == userId);

                if (request == null)
                    return Result<bool>.Failure("Friend request not found");

                if (request.Status != "Pending")
                    return Result<bool>.Failure("Can only cancel pending requests");

                await _unitOfWork.UserFriends.DeleteAsync(request);
                await _unitOfWork.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling friend request {RequestId}", requestId);
                return Result<bool>.Failure("Failed to cancel friend request");
            }
        }

        public async Task<Result<bool>> CheckFriendshipStatusAsync(string userId, string targetUserId)
        {
            try
            {
                var isFriend = await _unitOfWork.UserFriends
                    .GetQueryable()
                    .AnyAsync(uf => 
                        ((uf.UserId == userId && uf.FriendId == targetUserId) ||
                         (uf.UserId == targetUserId && uf.FriendId == userId)) &&
                        uf.Status == "Accepted");

                return Result<bool>.Success(isFriend);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking friendship status");
                return Result<bool>.Failure("Failed to check friendship status");
            }
        }
    }
}
