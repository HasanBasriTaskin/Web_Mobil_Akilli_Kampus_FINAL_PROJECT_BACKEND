using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IMealNutritionDal : IGenericDal<MealNutrition>
    {
        Task<MealNutrition?> GetByMenuIdAsync(int menuId);
    }
}
