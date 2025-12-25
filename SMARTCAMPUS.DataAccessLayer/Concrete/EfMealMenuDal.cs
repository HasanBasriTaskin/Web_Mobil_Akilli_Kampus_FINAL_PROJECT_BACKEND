using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfMealMenuDal : GenericRepository<MealMenu>, IMealMenuDal
    {
        public EfMealMenuDal(CampusContext context) : base(context)
        {
        }

        public async Task<MealMenu?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.MealMenus
                .Include(m => m.Cafeteria)
                .Include(m => m.Nutrition)
                .Include(m => m.MenuItems)
                    .ThenInclude(mi => mi.FoodItem)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<MealMenu>> GetMenusAsync(DateTime? date, int? cafeteriaId, MealType? mealType)
        {
            var query = _context.MealMenus
                .Include(m => m.Cafeteria)
                .Include(m => m.MenuItems)
                .Where(m => m.IsActive && m.IsPublished)
                .AsQueryable();

            if (date.HasValue)
                query = query.Where(m => m.Date.Date == date.Value.Date);

            if (cafeteriaId.HasValue)
                query = query.Where(m => m.CafeteriaId == cafeteriaId.Value);

            if (mealType.HasValue)
                query = query.Where(m => m.MealType == mealType.Value);

            return await query
                .OrderByDescending(m => m.Date)
                .ThenBy(m => m.MealType)
                .ToListAsync();
        }

        public async Task<bool> ExistsForCafeteriaDateMealTypeAsync(int cafeteriaId, DateTime date, MealType mealType, int? excludeId = null)
        {
            var query = _context.MealMenus.Where(m =>
                m.CafeteriaId == cafeteriaId &&
                m.Date.Date == date.Date &&
                m.MealType == mealType &&
                m.IsActive);

            if (excludeId.HasValue)
                query = query.Where(m => m.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> HasActiveReservationsAsync(int menuId)
        {
            return await _context.MealReservations
                .AnyAsync(r => r.MenuId == menuId && r.Status == MealReservationStatus.Reserved);
        }
    }
}
