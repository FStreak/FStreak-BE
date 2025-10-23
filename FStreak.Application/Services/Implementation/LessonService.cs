using FStreak.Application.DTOs;
using FStreak.Application.Services.Interface;
using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using AutoMapper;

namespace FStreak.Application.Services.Implementation
{
    public class LessonService : ILessonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LessonService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<LessonReadDto>> CreateLessonAsync(LessonCreateDto createDto, string teacherId)
        {
            try
            {
                var lesson = _mapper.Map<Lesson>(createDto);
                lesson.CreatedById = teacherId;
                lesson.CreatedAt = DateTime.UtcNow;

                await _unitOfWork.Lessons.AddAsync(lesson);
                await _unitOfWork.SaveChangesAsync();

                var lessonWithTeacher = await _unitOfWork.Lessons.GetByIdWithTeacherAsync(lesson.Id);
                var result = _mapper.Map<LessonReadDto>(lessonWithTeacher);

                return Result<LessonReadDto>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<LessonReadDto>.Failure($"Failed to create lesson: {ex.Message}");
            }
        }

        public async Task<Result<LessonReadDto>> GetLessonByIdAsync(Guid id)
        {
            try
            {
                var lesson = await _unitOfWork.Lessons.GetByIdWithTeacherAsync(id);
                if (lesson == null)
                {
                    return Result<LessonReadDto>.Failure("Lesson not found");
                }

                var result = _mapper.Map<LessonReadDto>(lesson);
                return Result<LessonReadDto>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<LessonReadDto>.Failure($"Failed to get lesson: {ex.Message}");
            }
        }

        public async Task<Result<IEnumerable<LessonReadDto>>> GetLessonsByTeacherAsync(string teacherId)
        {
            try
            {
                var lessons = await _unitOfWork.Lessons.GetByTeacherIdAsync(teacherId);
                var result = _mapper.Map<IEnumerable<LessonReadDto>>(lessons);

                return Result<IEnumerable<LessonReadDto>>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<LessonReadDto>>.Failure($"Failed to get lessons: {ex.Message}");
            }
        }

        public async Task<Result<LessonReadDto>> UpdateLessonAsync(Guid id, LessonUpdateDto updateDto, string teacherId)
        {
            try
            {
                var lesson = await _unitOfWork.Lessons.GetByIdWithTeacherAsync(id);
                if (lesson == null)
                {
                    return Result<LessonReadDto>.Failure("Lesson not found");
                }

                if (lesson.CreatedById != teacherId)
                {
                    return Result<LessonReadDto>.Failure("You can only update your own lessons");
                }

                _mapper.Map(updateDto, lesson);
                lesson.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Lessons.UpdateAsync(lesson);
                await _unitOfWork.SaveChangesAsync();

                var updatedLesson = await _unitOfWork.Lessons.GetByIdWithTeacherAsync(id);
                var result = _mapper.Map<LessonReadDto>(updatedLesson);

                return Result<LessonReadDto>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<LessonReadDto>.Failure($"Failed to update lesson: {ex.Message}");
            }
        }

        public async Task<Result<bool>> DeleteLessonAsync(Guid id, string teacherId)
        {
            try
            {
                var lesson = await _unitOfWork.Lessons.GetByIdWithTeacherAsync(id);
                if (lesson == null)
                {
                    return Result<bool>.Failure("Lesson not found");
                }

                if (lesson.CreatedById != teacherId)
                {
                    return Result<bool>.Failure("You can only delete your own lessons");
                }

                await _unitOfWork.Lessons.DeleteAsync(lesson);
                await _unitOfWork.SaveChangesAsync();

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Failed to delete lesson: {ex.Message}");
            }
        }
    }
}
