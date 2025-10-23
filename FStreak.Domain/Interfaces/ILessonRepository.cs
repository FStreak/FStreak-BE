using FStreak.Domain.Entities;

namespace FStreak.Domain.Interfaces
{
    public interface ILessonRepository : IRepository<Lesson>
    {
        Task<IEnumerable<Lesson>> GetByTeacherIdAsync(string teacherId);
        Task<Lesson?> GetByIdWithTeacherAsync(Guid id);
    }
}
