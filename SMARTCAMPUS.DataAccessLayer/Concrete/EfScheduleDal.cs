using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfScheduleDal : GenericRepository<Schedule>, IScheduleDal
    {
        public EfScheduleDal(CampusContext context) : base(context)
        {
        }

        public async Task<Schedule?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Schedules
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Course)
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Instructor)
                    .ThenInclude(i => i.User)
                .Include(s => s.Classroom)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<Schedule>> GetBySectionIdAsync(int sectionId)
        {
            return await _context.Schedules
                .Include(s => s.Classroom)
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Course)
                .Where(s => s.SectionId == sectionId && s.IsActive)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<List<Schedule>> GetByClassroomIdAsync(int classroomId, DayOfWeek? dayOfWeek = null)
        {
            var query = _context.Schedules
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Course)
                .Where(s => s.ClassroomId == classroomId && s.IsActive);

            if (dayOfWeek.HasValue)
                query = query.Where(s => s.DayOfWeek == dayOfWeek.Value);

            return await query
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<List<Schedule>> GetByInstructorIdAsync(int facultyId, DayOfWeek? dayOfWeek = null)
        {
            var query = _context.Schedules
                .Include(s => s.Classroom)
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Course)
                .Where(s => s.Section.InstructorId == facultyId && s.IsActive);

            if (dayOfWeek.HasValue)
                query = query.Where(s => s.DayOfWeek == dayOfWeek.Value);

            return await query
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<List<Schedule>> GetConflictsAsync(int classroomId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeId = null)
        {
            var query = _context.Schedules
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Course)
                .Where(s => s.ClassroomId == classroomId &&
                            s.DayOfWeek == dayOfWeek &&
                            s.IsActive &&
                            ((s.StartTime < endTime && s.EndTime > startTime)));

            if (excludeId.HasValue)
                query = query.Where(s => s.Id != excludeId.Value);

            return await query.ToListAsync();
        }

        public async Task<bool> HasConflictAsync(int classroomId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeId = null)
        {
            var query = _context.Schedules
                .Where(s => s.ClassroomId == classroomId &&
                            s.DayOfWeek == dayOfWeek &&
                            s.IsActive &&
                            ((s.StartTime < endTime && s.EndTime > startTime)));

            if (excludeId.HasValue)
                query = query.Where(s => s.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<Schedule?> GetConflictingScheduleAsync(int classroomId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeId = null)
        {
            var query = _context.Schedules
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Course)
                .Where(s => s.ClassroomId == classroomId &&
                           s.DayOfWeek == dayOfWeek &&
                           s.IsActive &&
                           (excludeId == null || s.Id != excludeId) &&
                           s.StartTime < endTime && s.EndTime > startTime);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Schedule?> GetSectionConflictAsync(int sectionId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, int? excludeId = null)
        {
            var query = _context.Schedules
                .Where(s => s.SectionId == sectionId &&
                           s.DayOfWeek == dayOfWeek &&
                           s.IsActive &&
                           (excludeId == null || s.Id != excludeId) &&
                           s.StartTime < endTime && s.EndTime > startTime);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<Schedule>> GetSchedulesBySectionIdsAsync(List<int> sectionIds)
        {
              return await _context.Schedules
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Course)
                .Where(s => sectionIds.Contains(s.SectionId) && s.IsActive)
                .ToListAsync();
        }
    }
}
