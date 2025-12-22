using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Abstract
{
    public interface IMealReservationDal : IGenericDal<MealReservation>
    {
        Task<MealReservation?> GetByQRCodeAsync(string qrCode);
        Task<bool> ExistsForUserDateMealTypeAsync(string userId, DateTime date, MealType mealType);
        Task<List<MealReservation>> GetByUserAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<MealReservation>> GetByDateAsync(DateTime date, int? cafeteriaId = null, MealType? mealType = null);
        Task<int> GetDailyReservationCountAsync(string userId, DateTime date);
        Task<List<MealReservation>> GetExpiredReservationsAsync();
    }
}
