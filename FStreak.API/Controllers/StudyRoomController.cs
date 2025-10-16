using FStreak.Application.DTOs;
using FStreak.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace FStreak.API.Controllers
{
    [ApiController]
    [Route("api/studyrooms")]
    //[Authorize]
    public class StudyRoomController : ControllerBase
    {
        private readonly ILogger<StudyRoomController> _logger;
        private readonly IStudyRoomService _studyRoomService;
        private readonly IAgoraService _agoraService;
        
        public StudyRoomController(
            ILogger<StudyRoomController> logger, 
            IStudyRoomService studyRoomService,
            IAgoraService agoraService)
        {
            _logger = logger;
            _studyRoomService = studyRoomService;
            _agoraService = agoraService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomDto createDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var result = await _studyRoomService.CreateRoomAsync(createDto, userId);

            if (result.Succeeded)
                return Ok(result.Data);
            return BadRequest(result.Error);
        }

        [HttpGet("{roomId}")]
        public async Task<IActionResult> GetRoomById(int roomId)
        {
            var result = await _studyRoomService.GetRoomByIdAsync(roomId);

            if (result.Succeeded)
                return Ok(result.Data);
            return NotFound(result.Error);
        }

        [HttpGet("code/{roomCode}")]
        public async Task<IActionResult> GetRoomByCode(string roomCode)
        {
            var result = await _studyRoomService.GetRoomByCodeAsync(roomCode);

            if (result.Succeeded)
                return Ok(result.Data);
            return NotFound(result.Error);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveRooms()
        {
            var result = await _studyRoomService.GetActiveRoomsAsync();

            if (result.Succeeded)
                return Ok(result.Data);
            return BadRequest(result.Error);
        }

        [HttpPost("{roomId}/join")]
        public async Task<IActionResult> JoinRoom(int roomId, [FromQuery] bool includeTokens = true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            // Join the room first
            var joinResult = await _studyRoomService.JoinRoom(roomId, userId);
            if (!joinResult.Succeeded)
                return BadRequest(joinResult.Error);

            // If client doesn't need tokens, return room user info only
            if (!includeTokens)
                return Ok(joinResult.Data);

            // Generate Agora tokens
            var channelName = $"room_{roomId}";
            var tokenResult = await _agoraService.GenerateTokenAsync(channelName, userId);
            
            if (!tokenResult.Succeeded)
            {
                // Even if token generation fails, user has joined the room
                _logger.LogWarning($"Failed to generate Agora tokens for user {userId} in room {roomId}: {tokenResult.Error}");
                return Ok(new JoinRoomResponse
                {
                    RoomUser = joinResult.Data,
                    AgoraTokens = null
                });
            }

            // Return both room user info and Agora tokens
            return Ok(new JoinRoomResponse
            {
                RoomUser = joinResult.Data,
                AgoraTokens = tokenResult.Data
            });
        }

        [HttpPost("join/code/{roomCode}")]
        public async Task<IActionResult> JoinRoomByCode(string roomCode, [FromQuery] bool includeTokens = true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            // Join the room by code
            var joinResult = await _studyRoomService.JoinRoomByCodeAsync(roomCode, userId);
            if (!joinResult.Succeeded)
                return BadRequest(joinResult.Error);

            // If client doesn't need tokens, return room user info only
            if (!includeTokens)
                return Ok(joinResult.Data);

            // Get room info to generate channel name
            var roomResult = await _studyRoomService.GetRoomByCodeAsync(roomCode);
            if (!roomResult.Succeeded || roomResult.Data == null)
                return Ok(joinResult.Data); // Return without tokens if can't get room

            var channelName = $"room_{roomResult.Data.StudyRoomId}";
            var tokenResult = await _agoraService.GenerateTokenAsync(channelName, userId);
            
            if (!tokenResult.Succeeded)
            {
                _logger.LogWarning($"Failed to generate Agora tokens for user {userId} joining by code {roomCode}: {tokenResult.Error}");
                return Ok(new JoinRoomResponse
                {
                    RoomUser = joinResult.Data,
                    AgoraTokens = null
                });
            }

            return Ok(new JoinRoomResponse
            {
                RoomUser = joinResult.Data,
                AgoraTokens = tokenResult.Data
            });
        }

        [HttpPost("{roomId}/leave")]
        public async Task<IActionResult> LeaveRoom(int roomId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var result = await _studyRoomService.LeaveRoom(roomId, userId);

            if (result.Succeeded)
                return Ok(result.Data);
            return BadRequest(result.Error);
        }

        [HttpPost("{roomId}/end")]
        public async Task<IActionResult> EndRoom(int roomId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var result = await _studyRoomService.EndRoomAsync(roomId, userId);

            if (result.Succeeded)
                return Ok(result.Data);
            return BadRequest(result.Error);
        }

        // Refresh Agora tokens for existing room session
        //[HttpPost("{roomId}/refresh-tokens")]
        //public async Task<IActionResult> RefreshAgoraTokens(int roomId)
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    if (string.IsNullOrEmpty(userId))
        //        return Unauthorized("User not authenticated");

        //    // Verify user is in the room
        //    var roomResult = await _studyRoomService.GetRoomByIdAsync(roomId);
        //    if (!roomResult.Succeeded)
        //        return NotFound(roomResult.Error);

        //    var isInRoom = roomResult.Data?.RoomUsers?.Any(u => u.UserId == userId && u.IsOnline) ?? false;
        //    if (!isInRoom)
        //        return BadRequest("User is not in the room");

        //    // Generate new tokens
        //    var channelName = $"room_{roomId}";
        //    var tokenResult = await _agoraService.GenerateTokenAsync(roomId, roomUserId, userName);

        //    if (!tokenResult.Succeeded)
        //        return BadRequest(tokenResult.Error);

        //    return Ok(tokenResult.Data);
        //}
    }
}
