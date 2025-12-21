using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.DTOs.Meal.Reservation
{
    public class MealScanResultDto
    {
        public int ReservationId { get; set; }
        public string UserName { get; set; } = null!;
        public string CafeteriaName { get; set; } = null!;
        public MealType MealType { get; set; }
        public DateTime Date { get; set; }
        public bool IsValid { get; set; }
        public string? Message { get; set; }
    }
}
