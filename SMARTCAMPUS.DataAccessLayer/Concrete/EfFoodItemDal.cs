using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfFoodItemDal : GenericRepository<FoodItem>, IFoodItemDal
    {
        public EfFoodItemDal(CampusContext context) : base(context)
        {
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            var query = _context.FoodItems.Where(f => f.Name == name);
            if (excludeId.HasValue)
                query = query.Where(f => f.Id != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<List<FoodItem>> GetByCategoryAsync(MealItemCategory category)
        {
            return await _context.FoodItems
                .Where(f => f.IsActive && f.Category == category)
                .ToListAsync();
        }

        public async Task<bool> IsUsedInActiveMenuAsync(int foodItemId)
        {
            return await _context.MealMenuItems
                .AnyAsync(m => m.FoodItemId == foodItemId && m.Menu.IsActive);
        }
    }
}
