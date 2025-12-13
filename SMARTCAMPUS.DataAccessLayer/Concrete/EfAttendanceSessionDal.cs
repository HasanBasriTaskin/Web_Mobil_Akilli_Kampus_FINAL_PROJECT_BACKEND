using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfAttendanceSessionDal : GenericRepository<AttendanceSession>, IAttendanceSessionDal
    {
        public EfAttendanceSessionDal(CampusContext context) : base(context)
        {
        }

        public async Task<AttendanceSession?> GetSessionWithRecordsAsync(int sessionId)
        {
            return await _context.AttendanceSessions
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Course)
                .Include(s => s.Instructor)
                .Include(s => s.AttendanceRecords)
                    .ThenInclude(r => r.Student)
                        .ThenInclude(st => st.User)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        public async Task<IEnumerable<AttendanceSession>> GetSessionsBySectionAsync(int sectionId)
        {
            return await _context.AttendanceSessions
                .Where(s => s.SectionId == sectionId && s.IsActive)
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Course)
                .Include(s => s.Instructor)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<AttendanceSession>> GetSessionsByInstructorAsync(string instructorId)
        {
            return await _context.AttendanceSessions
                .Where(s => s.InstructorId == instructorId && s.IsActive)
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Course)
                .OrderByDescending(s => s.Date)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<AttendanceSession>> GetSessionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.AttendanceSessions
                .Where(s => s.Date >= startDate && s.Date <= endDate && s.IsActive)
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Course)
                .Include(s => s.Instructor)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
        }
    }
}

