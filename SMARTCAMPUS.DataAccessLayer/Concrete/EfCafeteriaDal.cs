using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfCafeteriaDal : GenericRepository<Cafeteria>, ICafeteriaDal
    {
        public EfCafeteriaDal(CampusContext context) : base(context)
        {
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.Cafeterias.Where(c => c.Name == name);
            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<bool> HasActiveMenusAsync(int cafeteriaId)
        {
            return await _context.MealMenus.AnyAsync(m => m.CafeteriaId == cafeteriaId && m.IsActive);
        }

        public async Task<bool> HasActiveReservationsAsync(int cafeteriaId)
        {
            return await _context.MealReservations
                .AnyAsync(r => r.CafeteriaId == cafeteriaId && r.Status == MealReservationStatus.Reserved);
        }
    }
}
