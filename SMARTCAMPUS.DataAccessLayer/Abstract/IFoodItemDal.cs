using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IFoodItemDal : IGenericDal<FoodItem>
    {
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        Task<List<FoodItem>> GetByCategoryAsync(MealItemCategory category);
        Task<bool> IsUsedInActiveMenuAsync(int foodItemId);
    }
}
