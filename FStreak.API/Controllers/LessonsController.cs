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
    public class LessonsController : ControllerBase
    {
        private readonly ILessonService _lessonService;

        public LessonsController(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateLesson([FromBody] LessonCreateDto createDto)
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(teacherId))
            {
                return Unauthorized("User ID not found");
            }

            var result = await _lessonService.CreateLessonAsync(createDto, teacherId);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return CreatedAtAction(nameof(GetLesson), new { id = result.Data!.Id }, result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLesson(Guid id)
        {
            var result = await _lessonService.GetLessonByIdAsync(id);
            if (!result.Succeeded)
            {
                return NotFound(result.Error);
            }

            return Ok(result.Data);
        }

        [HttpGet("teacher/{teacherId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetLessonsByTeacher(string teacherId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId) || currentUserId != teacherId)
            {
                return Forbid("You can only view your own lessons");
            }

            var result = await _lessonService.GetLessonsByTeacherAsync(teacherId);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Data);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateLesson(Guid id, [FromBody] LessonUpdateDto updateDto)
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(teacherId))
            {
                return Unauthorized("User ID not found");
            }

            var result = await _lessonService.UpdateLessonAsync(id, updateDto, teacherId);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return Ok(result.Data);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteLesson(Guid id)
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(teacherId))
            {
                return Unauthorized("User ID not found");
            }

            var result = await _lessonService.DeleteLessonAsync(id, teacherId);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return NoContent();
        }
    }
}
