using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfExcuseRequestDal : GenericRepository<ExcuseRequest>, IExcuseRequestDal
    {
        public EfExcuseRequestDal(CampusContext context) : base(context)
        {
        }

        public async Task<ExcuseRequest?> GetRequestWithDetailsAsync(int requestId, int instructorId)
        {
            return await _context.ExcuseRequests
                .Include(r => r.Session)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.Session.InstructorId == instructorId);
        }

        public async Task<IEnumerable<ExcuseRequest>> GetRequestsByStudentAsync(int studentId)
        {
            return await _context.ExcuseRequests
                .Where(r => r.StudentId == studentId)
                .Include(r => r.Session)
                    .ThenInclude(s => s.Section)
                        .ThenInclude(sec => sec.Course)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ExcuseRequest>> GetRequestsBySessionAsync(int sessionId)
        {
            return await _context.ExcuseRequests
                .Where(r => r.SessionId == sessionId)
                .Include(r => r.Student)
                    .ThenInclude(s => s.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<ExcuseRequest>> GetPendingRequestsAsync()
        {
            return await _context.ExcuseRequests
                .Where(r => r.Status == ExcuseRequestStatus.Pending)
                .Include(r => r.Student)
                    .ThenInclude(s => s.User)
                .Include(r => r.Session)
                    .ThenInclude(s => s.Section)
                        .ThenInclude(sec => sec.Course)
                .OrderBy(r => r.CreatedDate)
                .ToListAsync();
        }
        public async Task<IEnumerable<ExcuseRequest>> GetRequestsByInstructorAsync(int instructorId, int? sectionId)
        {
            var query = _context.ExcuseRequests
                .Include(r => r.Student)
                    .ThenInclude(s => s.User)
                .Include(r => r.Session)
                    .ThenInclude(s => s.Section)
                        .ThenInclude(sec => sec.Course)
                .Where(r => r.Session.InstructorId == instructorId);

            if (sectionId.HasValue)
                query = query.Where(r => r.Session.SectionId == sectionId.Value);

            return await query.OrderByDescending(r => r.CreatedDate).ToListAsync();
        }
    }
}
