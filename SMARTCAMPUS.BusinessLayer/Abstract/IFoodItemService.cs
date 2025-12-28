using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.FoodItem;
using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    public interface IFoodItemService
    {
        Task<Response<List<FoodItemDto>>> GetAllAsync(bool includeInactive = false);
        Task<Response<List<FoodItemDto>>> GetByCategoryAsync(MealItemCategory category);
        Task<Response<FoodItemDto>> GetByIdAsync(int id);
        Task<Response<FoodItemDto>> CreateAsync(FoodItemCreateDto dto);
        Task<Response<FoodItemDto>> UpdateAsync(int id, FoodItemUpdateDto dto);
        Task<Response<NoDataDto>> DeleteAsync(int id);
    }
}
