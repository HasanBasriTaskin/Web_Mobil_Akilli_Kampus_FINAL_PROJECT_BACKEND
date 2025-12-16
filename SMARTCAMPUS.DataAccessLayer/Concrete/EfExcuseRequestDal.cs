using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfExcuseRequestDal : GenericRepository<ExcuseRequest>, IExcuseRequestDal
    {
        private readonly CampusContext _context;

        public EfExcuseRequestDal(CampusContext context) : base(context)
        {
            _context = context;
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
    }
}
