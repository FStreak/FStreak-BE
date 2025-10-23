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
        public async Task<IActionResult> CreateLesson([
            FromForm] LessonCreateDto createDto,
            [FromForm] IFormFile? documentFile,
            [FromForm] IFormFile? videoFile)
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(teacherId))
            {
                return Unauthorized("User ID not found");
            }

            // Xử lý upload file tài liệu
            if (documentFile != null && documentFile.Length > 0)
            {
                // TODO: Lưu file lên server/cloud và lấy URL
                // Ví dụ: var documentUrl = await _fileService.SaveFileAsync(documentFile);
                createDto.DocumentUrl = $"/uploads/docs/{Guid.NewGuid()}_{documentFile.FileName}";
                createDto.DocumentType = System.IO.Path.GetExtension(documentFile.FileName)?.TrimStart('.').ToLower();
                // Lưu file vật lý (demo, cần thay bằng lưu cloud thực tế)
                var path = Path.Combine("wwwroot/uploads/docs", Path.GetFileName(createDto.DocumentUrl));
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await documentFile.CopyToAsync(stream);
                }
            }

            // Xử lý upload file video
            if (videoFile != null && videoFile.Length > 0)
            {
                createDto.VideoUrl = $"/uploads/videos/{Guid.NewGuid()}_{videoFile.FileName}";
                // Lưu file vật lý (demo, cần thay bằng lưu cloud thực tế)
                var path = Path.Combine("wwwroot/uploads/videos", Path.GetFileName(createDto.VideoUrl));
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await videoFile.CopyToAsync(stream);
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
            [FromForm] LessonUpdateDto updateDto,
            [FromForm] IFormFile? documentFile,
            [FromForm] IFormFile? videoFile)
        {
            var teacherId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(teacherId))
            {
                return Unauthorized("User ID not found");
            }

            // Xử lý upload file tài liệu
            if (documentFile != null && documentFile.Length > 0)
            {
                updateDto.DocumentUrl = $"/uploads/docs/{Guid.NewGuid()}_{documentFile.FileName}";
                updateDto.DocumentType = System.IO.Path.GetExtension(documentFile.FileName)?.TrimStart('.').ToLower();
                var path = Path.Combine("wwwroot/uploads/docs", Path.GetFileName(updateDto.DocumentUrl));
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await documentFile.CopyToAsync(stream);
                }
            }

            // Xử lý upload file video
            if (videoFile != null && videoFile.Length > 0)
            {
                updateDto.VideoUrl = $"/uploads/videos/{Guid.NewGuid()}_{videoFile.FileName}";
                var path = Path.Combine("wwwroot/uploads/videos", Path.GetFileName(updateDto.VideoUrl));
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await videoFile.CopyToAsync(stream);
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

