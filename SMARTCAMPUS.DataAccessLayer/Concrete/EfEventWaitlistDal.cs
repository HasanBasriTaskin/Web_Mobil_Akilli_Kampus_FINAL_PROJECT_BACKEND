using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfEventWaitlistDal : GenericRepository<EventWaitlist>, IEventWaitlistDal
    {
        public EfEventWaitlistDal(CampusContext context) : base(context)
        {
        }

        public async Task<EventWaitlist?> GetByEventAndUserAsync(int eventId, string userId)
        {
            return await _context.EventWaitlists
                .FirstOrDefaultAsync(w => w.EventId == eventId && w.UserId == userId && w.IsActive);
        }

        public async Task<EventWaitlist?> GetNextInQueueAsync(int eventId)
        {
            return await _context.EventWaitlists
                .Where(w => w.EventId == eventId && w.IsActive && !w.IsNotified)
                .OrderBy(w => w.QueuePosition)
                .FirstOrDefaultAsync();
        }

        public async Task<List<EventWaitlist>> GetByEventIdAsync(int eventId)
        {
            return await _context.EventWaitlists
                .Include(w => w.User)
                .Where(w => w.EventId == eventId && w.IsActive)
                .OrderBy(w => w.QueuePosition)
                .ToListAsync();
        }

        public async Task<bool> IsUserInWaitlistAsync(int eventId, string userId)
        {
            return await _context.EventWaitlists
                .AnyAsync(w => w.EventId == eventId && w.UserId == userId && w.IsActive);
        }

        public async Task<int> GetMaxPositionAsync(int eventId)
        {
            return await _context.EventWaitlists
                .Where(w => w.EventId == eventId && w.IsActive)
                .MaxAsync(w => (int?)w.QueuePosition) ?? 0;
        }
    }
}
