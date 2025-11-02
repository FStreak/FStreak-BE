using FStreak.Application.DTOs;
using FStreak.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FStreak.API.Controllers
{
    [ApiController]
    [Route("api/friends")]
    [Authorize]
    public class FriendsController : ControllerBase
    {
        private readonly IFriendService _friendService;
        private readonly ILogger<FriendsController> _logger;

        public FriendsController(IFriendService friendService, ILogger<FriendsController> logger)
        {
            _friendService = friendService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách bạn bè
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFriends()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _friendService.GetFriendsAsync(userId);

            if (result.Succeeded)
                return Ok(result.Data);

            return BadRequest(result.Error);
        }

        /// <summary>
        /// Lấy danh sách lời mời kết bạn (đã gửi và đã nhận)
        /// </summary>
        [HttpGet("requests")]
        public async Task<IActionResult> GetFriendRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _friendService.GetFriendRequestsAsync(userId);

            if (result.Succeeded)
                return Ok(result.Data);

            return BadRequest(result.Error);
        }

        /// <summary>
        /// Gửi lời mời kết bạn
        /// </summary>
        [HttpPost("request")]
        public async Task<IActionResult> SendFriendRequest([FromBody] SendFriendRequestDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _friendService.SendFriendRequestAsync(userId, dto.ReceiverId);

            if (result.Succeeded)
                return Ok(result.Data);

            return BadRequest(result.Error);
        }

        /// <summary>
        /// Chấp nhận hoặc từ chối lời mời kết bạn
        /// </summary>
        [HttpPost("respond")]
        public async Task<IActionResult> RespondToFriendRequest([FromBody] RespondFriendRequestDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _friendService.RespondToFriendRequestAsync(dto.RequestId, userId, dto.Accept);

            if (result.Succeeded)
                return Ok(new { message = dto.Accept ? "Friend request accepted" : "Friend request rejected", data = result.Data });

            return BadRequest(result.Error);
        }

        /// <summary>
        /// Hủy kết bạn
        /// </summary>
        [HttpDelete("{friendshipId}")]
        public async Task<IActionResult> Unfriend(int friendshipId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _friendService.UnfriendAsync(friendshipId, userId);

            if (result.Succeeded)
                return Ok(new { message = "Unfriended successfully" });

            return BadRequest(result.Error);
        }

        /// <summary>
        /// Hủy lời mời kết bạn đã gửi
        /// </summary>
        [HttpDelete("request/{requestId}")]
        public async Task<IActionResult> CancelFriendRequest(int requestId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _friendService.CancelFriendRequestAsync(requestId, userId);

            if (result.Succeeded)
                return Ok(new { message = "Friend request canceled" });

            return BadRequest(result.Error);
        }

        /// <summary>
        /// Kiểm tra trạng thái bạn bè với user khác
        /// </summary>
        [HttpGet("status/{targetUserId}")]
        public async Task<IActionResult> CheckFriendshipStatus(string targetUserId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _friendService.CheckFriendshipStatusAsync(userId, targetUserId);

            if (result.Succeeded)
                return Ok(new { isFriend = result.Data });

            return BadRequest(result.Error);
        }
    }
}
