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
        private readonly ICloudinaryService _cloudinaryService;

        public LessonsController(ILessonService lessonService, ICloudinaryService cloudinaryService)
        {
            _lessonService = lessonService;
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [RequestSizeLimit(100_000_000)] // 100MB
        public async Task<IActionResult> CreateLesson([FromForm] LessonCreateWithFileDto dto)
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(teacherId))
                return Unauthorized("User ID not found");

            var createDto = new LessonCreateDto
            {
                Title = dto.Title,
                Description = dto.Description,
                StartAt = dto.StartAt,
                DurationMinutes = dto.DurationMinutes,
                IsPublished = dto.IsPublished
            };

            // Upload document to Cloudinary
            if (dto.DocumentFile != null && dto.DocumentFile.Length > 0)
            {
                var docResult = await _cloudinaryService.UploadDocumentAsync(dto.DocumentFile, "lessons/documents");
                if (!docResult.Succeeded)
                    return BadRequest($"Document upload failed: {docResult.Error}");

                createDto.DocumentUrl = docResult.Data!.SecureUrl;
                createDto.DocumentType = docResult.Data.Format;
            }

            // Upload video to Cloudinary
            if (dto.VideoFile != null && dto.VideoFile.Length > 0)
            {
                var videoResult = await _cloudinaryService.UploadVideoAsync(dto.VideoFile, "lessons/videos");
                if (!videoResult.Succeeded)
                    return BadRequest($"Video upload failed: {videoResult.Error}");

                createDto.VideoUrl = videoResult.Data!.SecureUrl;
            }

            var result = await _lessonService.CreateLessonAsync(createDto, teacherId);
            if (!result.Succeeded)
                return BadRequest(result.Error);

            return CreatedAtAction(nameof(GetLesson), new { id = result.Data!.Id }, result.Data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLesson(Guid id)
        {
            var result = await _lessonService.GetLessonByIdAsync(id);
            if (!result.Succeeded)
                return NotFound(result.Error);

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
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(teacherId))
                return Unauthorized("User ID not found");

            var updateDto = new LessonUpdateDto
            {
                Title = dto.Title,
                Description = dto.Description,
                StartAt = dto.StartAt,
                DurationMinutes = dto.DurationMinutes,
                IsPublished = dto.IsPublished
            };

            // Upload document to Cloudinary
            if (dto.DocumentFile != null && dto.DocumentFile.Length > 0)
            {
                var docResult = await _cloudinaryService.UploadDocumentAsync(dto.DocumentFile, "lessons/documents");
                if (!docResult.Succeeded)
                    return BadRequest($"Document upload failed: {docResult.Error}");

                updateDto.DocumentUrl = docResult.Data!.SecureUrl;
                updateDto.DocumentType = docResult.Data.Format;
            }

            // Upload video to Cloudinary
            if (dto.VideoFile != null && dto.VideoFile.Length > 0)
            {
                var videoResult = await _cloudinaryService.UploadVideoAsync(dto.VideoFile, "lessons/videos");
                if (!videoResult.Succeeded)
                    return BadRequest($"Video upload failed: {videoResult.Error}");

                updateDto.VideoUrl = videoResult.Data!.SecureUrl;
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
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(teacherId))
                return Unauthorized("User ID not found");

            var result = await _lessonService.DeleteLessonAsync(id, teacherId);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return NoContent();
        }
    }
}

