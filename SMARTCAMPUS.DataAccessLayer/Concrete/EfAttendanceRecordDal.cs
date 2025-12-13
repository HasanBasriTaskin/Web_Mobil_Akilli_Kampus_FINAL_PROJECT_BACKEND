using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfAttendanceRecordDal : GenericRepository<AttendanceRecord>, IAttendanceRecordDal
    {
        public EfAttendanceRecordDal(CampusContext context) : base(context)
        {
        }

        public async Task<AttendanceRecord?> GetRecordBySessionAndStudentAsync(int sessionId, int studentId)
        {
            return await _context.AttendanceRecords
                .Include(r => r.Session)
                .Include(r => r.Student)
                    .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(r => r.SessionId == sessionId && r.StudentId == studentId);
        }

        public async Task<IEnumerable<AttendanceRecord>> GetRecordsBySessionAsync(int sessionId)
        {
            return await _context.AttendanceRecords
                .Where(r => r.SessionId == sessionId && r.IsActive)
                .Include(r => r.Student)
                    .ThenInclude(s => s.User)
                .OrderBy(r => r.Student.StudentNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<AttendanceRecord>> GetRecordsByStudentAsync(int studentId)
        {
            return await _context.AttendanceRecords
                .Where(r => r.StudentId == studentId && r.IsActive)
                .Include(r => r.Session)
                    .ThenInclude(s => s.Section)
                        .ThenInclude(sec => sec.Course)
                .OrderByDescending(r => r.Session.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<AttendanceRecord>> GetRecordsBySectionAsync(int sectionId)
        {
            return await _context.AttendanceRecords
                .Where(r => r.Session.SectionId == sectionId && r.IsActive)
                .Include(r => r.Session)
                .Include(r => r.Student)
                    .ThenInclude(s => s.User)
                .OrderByDescending(r => r.Session.Date)
                .ThenBy(r => r.Student.StudentNumber)
                .ToListAsync();
        }
    }
}

