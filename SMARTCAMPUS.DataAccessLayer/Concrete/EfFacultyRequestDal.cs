using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfFacultyRequestDal : GenericRepository<FacultyCourseSectionRequest>, IFacultyRequestDal
    {
        public EfFacultyRequestDal(CampusContext context) : base(context)
        {
        }

        public async Task<List<FacultyCourseSectionRequest>> GetPendingRequestsAsync()
        {
            return await _context.FacultyCourseSectionRequests
                .Where(r => r.Status == "Pending")
                .Include(r => r.Faculty)
                    .ThenInclude(f => f.User)
                .Include(r => r.Faculty)
                    .ThenInclude(f => f.Department)
                .Include(r => r.Section)
                    .ThenInclude(s => s.Course)
                .OrderBy(r => r.RequestDate)
                .ToListAsync();
        }

        public async Task<List<FacultyCourseSectionRequest>> GetRequestsByFacultyAsync(int facultyId)
        {
            return await _context.FacultyCourseSectionRequests
                .Where(r => r.FacultyId == facultyId)
                .Include(r => r.Section)
                    .ThenInclude(s => s.Course)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();
        }

        public async Task<FacultyCourseSectionRequest?> GetRequestWithDetailsAsync(int id)
        {
            return await _context.FacultyCourseSectionRequests
                .Include(r => r.Faculty)
                    .ThenInclude(f => f.User)
                .Include(r => r.Faculty)
                    .ThenInclude(f => f.Department)
                .Include(r => r.Section)
                    .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> HasPendingRequestAsync(int facultyId, int sectionId)
        {
            return await _context.FacultyCourseSectionRequests
                .AnyAsync(r => r.FacultyId == facultyId 
                            && r.SectionId == sectionId 
                            && r.Status == "Pending");
        }
    }
}
