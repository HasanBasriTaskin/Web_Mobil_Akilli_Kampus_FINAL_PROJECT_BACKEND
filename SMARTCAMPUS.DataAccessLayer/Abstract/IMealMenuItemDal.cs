using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IMealMenuItemDal : IGenericDal<MealMenuItem>
    {
        Task<bool> ExistsAsync(int menuId, int foodItemId);
        Task<int> GetMaxOrderIndexAsync(int menuId);
        Task<MealMenuItem?> GetByMenuAndFoodItemAsync(int menuId, int foodItemId);
        Task RemoveByMenuIdAsync(int menuId);
    }
}
