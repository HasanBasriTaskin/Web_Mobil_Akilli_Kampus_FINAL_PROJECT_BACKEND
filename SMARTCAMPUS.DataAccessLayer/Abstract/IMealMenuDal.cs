using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IMealMenuDal : IGenericDal<MealMenu>
    {
        Task<MealMenu?> GetByIdWithDetailsAsync(int id);
        Task<List<MealMenu>> GetMenusAsync(DateTime? date, int? cafeteriaId, MealType? mealType);
        Task<bool> ExistsForCafeteriaDateMealTypeAsync(int cafeteriaId, DateTime date, MealType mealType, int? excludeId = null);
        Task<bool> HasActiveReservationsAsync(int menuId);
    }
}
