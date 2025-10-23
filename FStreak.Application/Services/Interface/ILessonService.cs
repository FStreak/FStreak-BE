using FStreak.Application.DTOs;
using FStreak.Domain.Entities;

namespace FStreak.Application.Services.Interface
{
    public interface ILessonService
    {
        Task<Result<LessonReadDto>> CreateLessonAsync(LessonCreateDto createDto, string teacherId);
        Task<Result<LessonReadDto>> GetLessonByIdAsync(Guid id);
        Task<Result<IEnumerable<LessonReadDto>>> GetLessonsByTeacherAsync(string teacherId);
        Task<Result<LessonReadDto>> UpdateLessonAsync(Guid id, LessonUpdateDto updateDto, string teacherId);
        Task<Result<bool>> DeleteLessonAsync(Guid id, string teacherId);
    }
}
