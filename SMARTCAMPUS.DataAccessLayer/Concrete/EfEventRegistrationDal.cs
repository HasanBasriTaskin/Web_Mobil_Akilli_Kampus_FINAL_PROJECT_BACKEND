using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfEventRegistrationDal : GenericRepository<EventRegistration>, IEventRegistrationDal
    {
        public EfEventRegistrationDal(CampusContext context) : base(context)
        {
        }

        public async Task<EventRegistration?> GetByEventAndUserAsync(int eventId, string userId)
        {
            return await _context.EventRegistrations
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId && r.IsActive);
        }

        public async Task<EventRegistration?> GetByQRCodeAsync(string qrCode)
        {
            return await _context.EventRegistrations
                .Include(r => r.Event)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.QRCode == qrCode);
        }

        public async Task<List<EventRegistration>> GetByEventIdAsync(int eventId)
        {
            return await _context.EventRegistrations
                .Include(r => r.User)
                .Where(r => r.EventId == eventId && r.IsActive)
                .ToListAsync();
        }

        public async Task<List<EventRegistration>> GetByUserIdAsync(string userId)
        {
            return await _context.EventRegistrations
                .Include(r => r.Event)
                    .ThenInclude(e => e.Category)
                .Where(r => r.UserId == userId && r.IsActive)
                .OrderByDescending(r => r.Event.StartDate)
                .ToListAsync();
        }

        public async Task<bool> IsUserRegisteredAsync(int eventId, string userId)
        {
            return await _context.EventRegistrations
                .AnyAsync(r => r.EventId == eventId && r.UserId == userId && r.IsActive);
        }
    }
}
