using FStreak.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FStreak.API.Controllers
{
    [ApiController]
    [Route("api/agora")]
    [Authorize]

    public class AgoraController : ControllerBase
    {
        private readonly IAgoraService _agoraService;
        public AgoraController(IAgoraService agoraService)
        {
            _agoraService = agoraService;
        }

        [HttpGet("token")]
        public async Task<IActionResult> GetToken([FromQuery] string channelName)
        {
            // Lấy userId từ token JWT
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(string.IsNullOrEmpty(channelName))
                return BadRequest("Channel name is required.");

            var result = await _agoraService.GenerateTokenAsync(channelName, userId);
            if (result.Succeeded)
            {
                return Ok(result.Data);
            }
            else
            {
                return StatusCode(500, result.Error);
            }
        }

    }
}
