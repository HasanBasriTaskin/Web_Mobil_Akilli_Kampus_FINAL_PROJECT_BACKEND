using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfAnnouncementDal : GenericRepository<Announcement>, IAnnouncementDal
    {
        public EfAnnouncementDal(CampusContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Announcement>> GetActiveAnnouncementsAsync(string? targetAudience = null, int? departmentId = null)
        {
            var query = _context.Announcements
                .Where(a => a.IsActive &&
                           (a.ExpiryDate == null || a.ExpiryDate >= DateTime.UtcNow) &&
                           a.PublishDate <= DateTime.UtcNow);

            if (!string.IsNullOrEmpty(targetAudience))
            {
                query = query.Where(a => a.TargetAudience == targetAudience || a.TargetAudience == "All");
            }

            if (departmentId.HasValue)
            {
                query = query.Where(a => a.DepartmentId == departmentId || a.DepartmentId == null);
            }

            return await query
                .Include(a => a.Department)
                .Include(a => a.CreatedBy)
                .OrderByDescending(a => a.IsImportant)
                .ThenByDescending(a => a.PublishDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetImportantAnnouncementsAsync()
        {
            return await _context.Announcements
                .Where(a => a.IsActive &&
                           a.IsImportant &&
                           (a.ExpiryDate == null || a.ExpiryDate >= DateTime.UtcNow) &&
                           a.PublishDate <= DateTime.UtcNow)
                .Include(a => a.Department)
                .Include(a => a.CreatedBy)
                .OrderByDescending(a => a.PublishDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetAnnouncementsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Announcements
                .Where(a => a.IsActive &&
                           a.PublishDate >= startDate &&
                           a.PublishDate <= endDate)
                .Include(a => a.Department)
                .Include(a => a.CreatedBy)
                .OrderByDescending(a => a.PublishDate)
                .ToListAsync();
        }
    }
}
