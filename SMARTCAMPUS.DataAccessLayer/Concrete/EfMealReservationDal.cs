using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfMealReservationDal : GenericRepository<MealReservation>, IMealReservationDal
    {
        public EfMealReservationDal(CampusContext context) : base(context)
        {
        }

        public async Task<MealReservation?> GetByQRCodeAsync(string qrCode)
        {
            return await _context.MealReservations
                .Include(r => r.User)
                .Include(r => r.Menu)
                    .ThenInclude(m => m.Cafeteria)
                .FirstOrDefaultAsync(r => r.QRCode == qrCode);
        }

        public async Task<bool> ExistsForUserDateMealTypeAsync(string userId, DateTime date, MealType mealType)
        {
            return await _context.MealReservations
                .AnyAsync(r => r.UserId == userId && r.Date.Date == date.Date && r.MealType == mealType && r.Status == MealReservationStatus.Reserved);
        }

        public async Task<List<MealReservation>> GetByUserAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.MealReservations
                .Include(r => r.Menu)
                    .ThenInclude(m => m.Cafeteria)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Date);

            if (fromDate.HasValue)
                query = (IOrderedQueryable<MealReservation>)query.Where(r => r.Date >= fromDate.Value);

            if (toDate.HasValue)
                query = (IOrderedQueryable<MealReservation>)query.Where(r => r.Date <= toDate.Value);

            return await query.ToListAsync();
        }

        public async Task<List<MealReservation>> GetByDateAsync(DateTime date, int? cafeteriaId = null, MealType? mealType = null)
        {
            var query = _context.MealReservations
                .Include(r => r.User)
                .Include(r => r.Menu)
                .Where(r => r.Date.Date == date.Date);

            if (cafeteriaId.HasValue)
                query = query.Where(r => r.CafeteriaId == cafeteriaId.Value);

            if (mealType.HasValue)
                query = query.Where(r => r.MealType == mealType.Value);

            return await query.ToListAsync();
        }

        public async Task<int> GetDailyReservationCountAsync(string userId, DateTime date)
        {
            return await _context.MealReservations
                .CountAsync(r => r.UserId == userId && r.Date.Date == date.Date && r.Status == MealReservationStatus.Reserved);
        }

        public async Task<List<MealReservation>> GetExpiredReservationsAsync()
        {
            var cutoffTime = DateTime.UtcNow;
            return await _context.MealReservations
                .Where(r => r.Status == MealReservationStatus.Reserved && r.Date < cutoffTime.Date)
                .ToListAsync();
        }
    }
}
