using Microsoft.AspNetCore.Mvc;
using FStreak.Application.Services;

namespace FStreak.API.Controllers
{
    [ApiController]
    [Route("api/streaks")]
    public class StreakController : ControllerBase
    {
        private readonly IStreakService _streakService;

        public StreakController(IStreakService streakService)
        {
            _streakService = streakService;
        }

        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn([FromQuery] int userId)
        {
            var (success, currentStreak, longestStreak) = await _streakService.CheckInAsync(userId);
            
            if (!success)
            {
                return BadRequest(new { 
                    success = false, 
                    message = "Already checked in today or user not found" 
                });
            }

            return Ok(new
            {
                success = true,
                currentStreak,
                longestStreak
            });
        }

        [HttpGet("{userId}/current")]
        public async Task<IActionResult> GetCurrentStreak(int userId)
        {
            var streak = await _streakService.GetCurrentStreakAsync(userId);
            return Ok(new { currentStreak = streak });
        }

        [HttpGet("{userId}/longest")]
        public async Task<IActionResult> GetLongestStreak(int userId)
        {
            var streak = await _streakService.GetLongestStreakAsync(userId);
            return Ok(new { longestStreak = streak });
        }
    }
}