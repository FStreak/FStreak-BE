using System;
using System.Security.Claims;
using FStreak.Application.DTOs;
using FStreak.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FStreak.Application;

namespace FStreak.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StreaksController : ControllerBase
    {
        private readonly IStreakService _streakService;

        public StreaksController(IStreakService streakService)
        {
            _streakService = streakService;
        }

        /// <summary>
        /// Get current user's streak information
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(StreakInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<StreakInfoDto>> GetMyStreak()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _streakService.GetUserStreakAsync(userId);
            if (!result.Succeeded)
                return BadRequest(new { message = result.Error });

            return Ok(result.Data);
        }

        /// <summary>
        /// Check in for today's streak
        /// </summary>
        [HttpPost("check-in")]
        [ProducesResponseType(typeof(StreakInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<StreakInfoDto>> CheckIn([FromBody] StreakCheckInDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Generate a unique idempotency key if not provided
            var idempotencyKey = Request.Headers["Idempotency-Key"].FirstOrDefault() 
                ?? Guid.NewGuid().ToString();

            var result = await _streakService.CheckInAsync(userId, request, idempotencyKey);
            if (!result.Succeeded)
                return BadRequest(new { message = result.Error });

            // Set the used idempotency key in response header
            Response.Headers["Idempotency-Key"] = idempotencyKey;
            return Ok(result.Data);
        }

        /// <summary>
        /// Get streak leaderboard
        /// </summary>
        [HttpGet("leaderboard")]
        [ProducesResponseType(typeof(StreakLeaderboardDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<StreakLeaderboardDto>> GetLeaderboard(
            [FromQuery] LeaderboardScope scope = LeaderboardScope.Global,
            [FromQuery] LeaderboardPeriod period = LeaderboardPeriod.Week,
            [FromQuery] int? groupId = null)
        {
            var request = new LeaderboardRequestDto
            {
                Scope = scope,
                Period = period,
                GroupId = groupId
            };

            var result = await _streakService.GetLeaderboardAsync(request);
            if (!result.Succeeded)
                return BadRequest(new { message = result.Error });

            return Ok(result.Data);
        }
    }
}