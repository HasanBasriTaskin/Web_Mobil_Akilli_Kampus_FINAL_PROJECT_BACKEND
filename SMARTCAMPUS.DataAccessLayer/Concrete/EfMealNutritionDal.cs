using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Concrete
{
    public class EfMealNutritionDal : GenericRepository<MealNutrition>, IMealNutritionDal
    {
        public EfMealNutritionDal(CampusContext context) : base(context)
        {
        }

        public async Task<MealNutrition?> GetByMenuIdAsync(int menuId)
        {
            return await _context.MealNutritions.FirstOrDefaultAsync(n => n.MenuId == menuId);
        }
    }
}
