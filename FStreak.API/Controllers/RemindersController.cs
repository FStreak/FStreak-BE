using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FStreak.Application.DTOs;
using FStreak.Application.Services.Interface;

namespace FStreak.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RemindersController : ControllerBase
    {
        private readonly IReminderService _reminderService;
        private readonly ILogger<RemindersController> _logger;

        public RemindersController(
            IReminderService reminderService,
            ILogger<RemindersController> logger)
        {
            _reminderService = reminderService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ReminderDto>>> GetUserReminders()
        {
            var userId = User.FindFirst("sub")?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var reminders = await _reminderService.GetUserRemindersAsync(userId);
            return Ok(reminders);
        }

        [HttpPost]
        public async Task<ActionResult<ReminderDto>> CreateReminder([FromBody] CreateReminderDto dto)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var reminder = await _reminderService.CreateReminderAsync(userId, dto);
                return CreatedAtAction(nameof(GetUserReminders), new { }, reminder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reminder");
                return BadRequest(new { message = "Could not create reminder" });
            }
        }

        [HttpPatch("{reminderId}")]
        public async Task<ActionResult<ReminderDto>> UpdateReminder(
            int reminderId, 
            [FromBody] UpdateReminderDto dto)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var reminder = await _reminderService.UpdateReminderAsync(userId, reminderId, dto);
                return Ok(reminder);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reminder");
                return BadRequest(new { message = "Could not update reminder" });
            }
        }

        [HttpDelete("{reminderId}")]
        public async Task<ActionResult> DeleteReminder(int reminderId)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var deleted = await _reminderService.DeleteReminderAsync(userId, reminderId);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost("{reminderId}/toggle")]
        public async Task<ActionResult> ToggleReminder(int reminderId, [FromBody] bool enable)
        {
            var userId = User.FindFirst("sub")?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var updated = await _reminderService.ToggleReminderAsync(userId, reminderId, enable);
            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}