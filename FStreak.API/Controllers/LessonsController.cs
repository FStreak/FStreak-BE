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
        [RequestSizeLimit(50_000_000)] // 50MB
        public async Task<IActionResult> CreateLesson([FromForm] FStreak.Application.DTOs.LessonCreateWithFileDto dto)
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(teacherId))
            {
                return Unauthorized("User ID not found");
            }

            var createDto = new LessonCreateDto
            {
                Title = dto.Title,
                Description = dto.Description,
                StartAt = dto.StartAt,
                DurationMinutes = dto.DurationMinutes,
                IsPublished = dto.IsPublished
            };

            // Xử lý upload file tài liệu
            if (dto.DocumentFile != null && dto.DocumentFile.Length > 0)
            {
                createDto.DocumentUrl = $"/uploads/docs/{Guid.NewGuid()}_{dto.DocumentFile.FileName}";
                createDto.DocumentType = System.IO.Path.GetExtension(dto.DocumentFile.FileName)?.TrimStart('.').ToLower();
                var path = Path.Combine("wwwroot/uploads/docs", Path.GetFileName(createDto.DocumentUrl));
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await dto.DocumentFile.CopyToAsync(stream);
                }
            }

            // Xử lý upload file video
            if (dto.VideoFile != null && dto.VideoFile.Length > 0)
            {
                createDto.VideoUrl = $"/uploads/videos/{Guid.NewGuid()}_{dto.VideoFile.FileName}";
                var path = Path.Combine("wwwroot/uploads/videos", Path.GetFileName(createDto.VideoUrl));
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await dto.VideoFile.CopyToAsync(stream);
                }
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
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> UpdateLesson(
            Guid id,
            [FromForm] FStreak.Application.DTOs.LessonUpdateWithFileDto dto)
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(teacherId))
            {
                return Unauthorized("User ID not found");
            }

            var updateDto = new LessonUpdateDto
            {
                Title = dto.Title,
                Description = dto.Description,
                StartAt = dto.StartAt,
                DurationMinutes = dto.DurationMinutes,
                IsPublished = dto.IsPublished
            };

            // Xử lý upload file tài liệu
            if (dto.DocumentFile != null && dto.DocumentFile.Length > 0)
            {
                updateDto.DocumentUrl = $"/uploads/docs/{Guid.NewGuid()}_{dto.DocumentFile.FileName}";
                updateDto.DocumentType = System.IO.Path.GetExtension(dto.DocumentFile.FileName)?.TrimStart('.').ToLower();
                var path = Path.Combine("wwwroot/uploads/docs", Path.GetFileName(updateDto.DocumentUrl));
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await dto.DocumentFile.CopyToAsync(stream);
                }
            }

            // Xử lý upload file video
            if (dto.VideoFile != null && dto.VideoFile.Length > 0)
            {
                updateDto.VideoUrl = $"/uploads/videos/{Guid.NewGuid()}_{dto.VideoFile.FileName}";
                var path = Path.Combine("wwwroot/uploads/videos", Path.GetFileName(updateDto.VideoUrl));
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await dto.VideoFile.CopyToAsync(stream);
                }
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

