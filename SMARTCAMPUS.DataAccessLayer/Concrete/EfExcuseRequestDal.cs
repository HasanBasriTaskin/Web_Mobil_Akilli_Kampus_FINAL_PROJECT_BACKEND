using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfExcuseRequestDal : GenericRepository<ExcuseRequest>, IExcuseRequestDal
    {
        public EfExcuseRequestDal(CampusContext context) : base(context)
        {
        }

        public async Task<ExcuseRequest?> GetRequestWithDetailsAsync(int requestId)
        {
            return await _context.ExcuseRequests
                .Include(r => r.Student)
                    .ThenInclude(s => s.User)
                .Include(r => r.Session)
                    .ThenInclude(s => s.Section)
                        .ThenInclude(sec => sec.Course)
                .Include(r => r.Reviewer)
                .FirstOrDefaultAsync(r => r.Id == requestId);
        }

        public async Task<IEnumerable<ExcuseRequest>> GetRequestsByStudentAsync(int studentId)
        {
            return await _context.ExcuseRequests
                .Where(r => r.StudentId == studentId && r.IsActive)
                .Include(r => r.Session)
                    .ThenInclude(s => s.Section)
                        .ThenInclude(sec => sec.Course)
                .Include(r => r.Reviewer)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ExcuseRequest>> GetRequestsBySessionAsync(int sessionId)
        {
            return await _context.ExcuseRequests
                .Where(r => r.SessionId == sessionId && r.IsActive)
                .Include(r => r.Student)
                    .ThenInclude(s => s.User)
                .Include(r => r.Reviewer)
                .OrderBy(r => r.Student.StudentNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<ExcuseRequest>> GetPendingRequestsAsync()
        {
            return await _context.ExcuseRequests
                .Where(r => r.Status == "Pending" && r.IsActive)
                .Include(r => r.Student)
                    .ThenInclude(s => s.User)
                .Include(r => r.Session)
                    .ThenInclude(s => s.Section)
                        .ThenInclude(sec => sec.Course)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }
    }
}

