using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.DTOs.Meal.Reservation
{
    public class MealReservationListDto
    {
        public int Id { get; set; }
        public string CafeteriaName { get; set; } = null!;
        public MealType MealType { get; set; }
        public string MealTypeName => MealType.ToString();
        public DateTime Date { get; set; }
        public MealReservationStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public string QRCode { get; set; } = null!;
    }
}
