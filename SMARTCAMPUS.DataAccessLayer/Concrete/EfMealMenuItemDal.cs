using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfMealMenuItemDal : GenericRepository<MealMenuItem>, IMealMenuItemDal
    {
        public EfMealMenuItemDal(CampusContext context) : base(context)
        {
        }

        public async Task<bool> ExistsAsync(int menuId, int foodItemId)
        {
            return await _context.MealMenuItems
                .AnyAsync(m => m.MenuId == menuId && m.FoodItemId == foodItemId);
        }

        public async Task<int> GetMaxOrderIndexAsync(int menuId)
        {
            return await _context.MealMenuItems
                .Where(m => m.MenuId == menuId)
                .MaxAsync(m => (int?)m.OrderIndex) ?? -1;
        }

        public async Task<MealMenuItem?> GetByMenuAndFoodItemAsync(int menuId, int foodItemId)
        {
            return await _context.MealMenuItems
                .FirstOrDefaultAsync(m => m.MenuId == menuId && m.FoodItemId == foodItemId);
        }

        public async Task RemoveByMenuIdAsync(int menuId)
        {
            var items = await _context.MealMenuItems.Where(m => m.MenuId == menuId).ToListAsync();
            _context.MealMenuItems.RemoveRange(items);
        }
    }
}
