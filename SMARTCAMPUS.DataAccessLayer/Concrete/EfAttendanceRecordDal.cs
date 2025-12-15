using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfAttendanceRecordDal : GenericRepository<AttendanceRecord>, IAttendanceRecordDal
    {
        private readonly CampusContext _context;

        public EfAttendanceRecordDal(CampusContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AttendanceRecord>> GetRecordsByStudentAsync(int studentId)
        {
            return await _context.AttendanceRecords
                .Where(r => r.StudentId == studentId)
                .Include(r => r.Session)
                    .ThenInclude(s => s.Section)
                        .ThenInclude(sec => sec.Course)
                .OrderByDescending(r => r.CheckInTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<AttendanceRecord>> GetRecordsBySessionAsync(int sessionId)
        {
            return await _context.AttendanceRecords
                .Where(r => r.SessionId == sessionId)
                .Include(r => r.Student)
                    .ThenInclude(s => s.User)
                .ToListAsync();
        }

        public async Task<bool> HasStudentCheckedInAsync(int sessionId, int studentId)
        {
            return await _context.AttendanceRecords
                .AnyAsync(r => r.SessionId == sessionId && r.StudentId == studentId);
        }
    }
}
