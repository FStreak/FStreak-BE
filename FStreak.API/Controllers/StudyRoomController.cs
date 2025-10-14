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
        public StudyRoomController(ILogger<StudyRoomController> logger, IStudyRoomService studyRoomService)
        {
            _logger = logger;
            _studyRoomService = studyRoomService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomDto createDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
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
        public async Task<IActionResult> JoinRoom(int roomId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _studyRoomService.JoinRoom(roomId, userId);

            if (result.Succeeded)
                return Ok(result.Data);
            return BadRequest(result.Error);
        }

        [HttpPost("join/code/{roomCode}")]
        public async Task<IActionResult> JoinRoomByCode(string roomCode)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _studyRoomService.JoinRoomByCodeAsync(roomCode, userId);

            if (result.Succeeded)
                return Ok(result.Data);
            return BadRequest(result.Error);
        }

        [HttpPost("{roomId}/leave")]
        public async Task<IActionResult> LeaveRoom(int roomId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _studyRoomService.LeaveRoom(roomId, userId);

            if (result.Succeeded)
                return Ok(result.Data);
            return BadRequest(result.Error);
        }

        [HttpPost("{roomId}/end")]
        public async Task<IActionResult> EndRoom(int roomId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _studyRoomService.EndRoomAsync(roomId, userId);

            if (result.Succeeded)
                return Ok(result.Data);
            return BadRequest(result.Error);
        }
    }
}
