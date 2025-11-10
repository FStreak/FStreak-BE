using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FStreak.Application.DTOs;
using FStreak.Application.Services.Interface;
using System.Security.Claims;

namespace FStreak.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AchievementsController : ControllerBase
    {
        private readonly IAchievementService _achievementService;

        public AchievementsController(IAchievementService achievementService)
        {
            _achievementService = achievementService;
        }

        /// <summary>
        /// Get all achievements for the current user
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(List<UserAchievementDto>), 200)]
        public async Task<IActionResult> GetMyAchievements()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found");
            }

            var result = await _achievementService.GetUserAchievementsAsync(userId);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Get all achievements for a specific user
        /// </summary>
        [HttpGet("users/{userId}")]
        [ProducesResponseType(typeof(List<UserAchievementDto>), 200)]
        public async Task<IActionResult> GetUserAchievements(string userId)
        {
            var result = await _achievementService.GetUserAchievementsAsync(userId);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Get a specific user achievement
        /// </summary>
        [HttpGet("users/{userId}/achievements/{achievementId}")]
        [ProducesResponseType(typeof(UserAchievementDto), 200)]
        public async Task<IActionResult> GetUserAchievement(string userId, Guid achievementId)
        {
            var result = await _achievementService.GetUserAchievementAsync(userId, achievementId);
            if (!result.Succeeded)
            {
                return NotFound(result.Error);
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Claim an achievement (mark as claimed)
        /// </summary>
        [HttpPost("users/{userId}/achievements/{achievementId}/claim")]
        [ProducesResponseType(typeof(UserAchievementDto), 200)]
        public async Task<IActionResult> ClaimAchievement(string userId, Guid achievementId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId) || currentUserId != userId)
            {
                return Forbid("You can only claim your own achievements");
            }

            var result = await _achievementService.ClaimAchievementAsync(userId, achievementId);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Get all available achievements
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<AchievementDto>), 200)]
        public async Task<IActionResult> GetAllAchievements()
        {
            var result = await _achievementService.GetAllAchievementsAsync();
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Get achievement by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AchievementDto), 200)]
        public async Task<IActionResult> GetAchievement(Guid id)
        {
            var result = await _achievementService.GetAchievementByIdAsync(id);
            if (!result.Succeeded)
            {
                return NotFound(result.Error);
            }

            return Ok(result.Data);
        }
    }
}

