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

        public async Task<AttendanceSession?> GetSessionWithRecordsAsync(int id)
        {
            return await _context.AttendanceSessions
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Course)
                .Include(s => s.Instructor)
                    .ThenInclude(i => i.User)
                .Include(s => s.AttendanceRecords)
                    .ThenInclude(r => r.Student)
                        .ThenInclude(st => st.User)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<AttendanceSession>> GetSessionsBySectionAsync(int sectionId)
        {
            return await _context.AttendanceSessions
                .Where(s => s.SectionId == sectionId)
                .OrderByDescending(s => s.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<AttendanceSession>> GetSessionsByInstructorAsync(int instructorId)
        {
            return await _context.AttendanceSessions
                .Where(s => s.InstructorId == instructorId)
                .Include(s => s.Section)
                    .ThenInclude(sec => sec.Course)
                .OrderByDescending(s => s.Date)
                .ToListAsync();
        }
    }
}
