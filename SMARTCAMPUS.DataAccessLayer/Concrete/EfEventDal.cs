using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfEventDal : GenericRepository<Event>, IEventDal
    {
        public EfEventDal(CampusContext context) : base(context)
        {
        }

        public async Task<Event?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Events
                .Include(e => e.Category)
                .Include(e => e.CreatedBy)
                .Include(e => e.Registrations)
                    .ThenInclude(r => r.User)
                .Include(e => e.Waitlists)
                    .ThenInclude(w => w.User)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Event>> GetEventsFilteredAsync(int? categoryId, DateTime? fromDate, DateTime? toDate, bool? isFree, string? searchQuery, int page, int pageSize)
        {
            var query = _context.Events
                .Include(e => e.Category)
                .Where(e => e.IsActive)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(e => e.CategoryId == categoryId.Value);

            if (fromDate.HasValue)
                query = query.Where(e => e.StartDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(e => e.EndDate <= toDate.Value);

            if (isFree.HasValue)
            {
                if (isFree.Value)
                    query = query.Where(e => e.Price == 0);
                else
                    query = query.Where(e => e.Price > 0);
            }

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(e => e.Title.Contains(searchQuery) || e.Description.Contains(searchQuery));

            return await query
                .OrderBy(e => e.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetEventsCountAsync(int? categoryId, DateTime? fromDate, DateTime? toDate, bool? isFree, string? searchQuery)
        {
            var query = _context.Events.Where(e => e.IsActive).AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(e => e.CategoryId == categoryId.Value);

            if (fromDate.HasValue)
                query = query.Where(e => e.StartDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(e => e.EndDate <= toDate.Value);

            if (isFree.HasValue)
            {
                if (isFree.Value)
                    query = query.Where(e => e.Price == 0);
                else
                    query = query.Where(e => e.Price > 0);
            }

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(e => e.Title.Contains(searchQuery) || e.Description.Contains(searchQuery));

            return await query.CountAsync();
        }
    }
}
