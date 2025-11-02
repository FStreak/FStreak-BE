using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using FStreak.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FStreak.Infrastructure.Repositories
{
    public class LessonRepository : Repository<Lesson>, ILessonRepository
    {
        public LessonRepository(FStreakDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Lesson>> GetByTeacherIdAsync(string teacherId)
        {
            return await _context.Lessons
                .Where(l => l.CreatedById == teacherId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<Lesson?> GetByIdWithTeacherAsync(Guid id)
        {
            return await _context.Lessons
                .Include(l => l.CreatedBy)
                .FirstOrDefaultAsync(l => l.Id == id);
        }
    }
}
