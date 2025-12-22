using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfEventCategoryDal : GenericRepository<EventCategory>, IEventCategoryDal
    {
        public EfEventCategoryDal(CampusContext context) : base(context)
        {
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.EventCategories.Where(c => c.Name == name);
            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<bool> HasActiveEventsAsync(int categoryId)
        {
            return await _context.Events.AnyAsync(e => e.CategoryId == categoryId && e.IsActive);
        }
    }
}
