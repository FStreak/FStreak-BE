using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FStreak.Application.DTOs;
using FStreak.Application.Services.Interface;

namespace FStreak.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/devices")]
    public class DevicesController : ControllerBase
    {
        private readonly IPushNotificationService _pushService;
        private readonly ILogger<DevicesController> _logger;

        public DevicesController(
            IPushNotificationService pushService,
            ILogger<DevicesController> logger)
        {
            _pushService = pushService;
            _logger = logger;
        }

        [HttpPost("push")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RegisterPushDevice([FromBody] RegisterPushDeviceDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _pushService.RegisterDeviceAsync(userId, dto);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.Error });
            }

            return CreatedAtAction(nameof(RegisterPushDevice), new { }, new { message = "Device registered successfully" });
        }

        [HttpDelete("push")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UnregisterPushDevice([FromQuery] string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint))
            {
                return BadRequest(new { message = "Endpoint is required" });
            }

            var result = await _pushService.UnregisterDeviceAsync(endpoint);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(new { message = "Device unregistered successfully" });
        }

        [HttpPost("push/test")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendTestNotification([FromBody] PushNotificationDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _pushService.SendNotificationAsync(userId, dto);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.Error });
            }

            return Ok(new { message = "Test notification sent successfully" });
        }
    }
}