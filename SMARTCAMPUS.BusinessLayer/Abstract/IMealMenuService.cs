using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Menu;
using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IMealMenuService
    {
        // Listeleme
        Task<Response<List<MealMenuListDto>>> GetMenusAsync(DateTime? date = null, int? cafeteriaId = null, MealType? mealType = null);
        Task<Response<MealMenuDto>> GetMenuByIdAsync(int id);
        
        // CRUD (Admin)
        Task<Response<MealMenuDto>> CreateMenuAsync(MealMenuCreateDto dto);
        Task<Response<MealMenuDto>> UpdateMenuAsync(int id, MealMenuCreateDto dto);
        Task<Response<NoDataDto>> PublishMenuAsync(int id);
        Task<Response<NoDataDto>> UnpublishMenuAsync(int id);
        Task<Response<NoDataDto>> DeleteMenuAsync(int id, bool force = false);
        
        // Menu Items
        Task<Response<NoDataDto>> AddFoodItemToMenuAsync(int menuId, int foodItemId);
        Task<Response<NoDataDto>> RemoveFoodItemFromMenuAsync(int menuId, int foodItemId);
    }
}
