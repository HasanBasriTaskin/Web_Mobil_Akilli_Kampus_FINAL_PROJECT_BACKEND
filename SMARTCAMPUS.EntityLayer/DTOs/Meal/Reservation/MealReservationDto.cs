using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.DTOs.Meal.Reservation
{
    public class MealReservationDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public int MenuId { get; set; }
        public int CafeteriaId { get; set; }
        public string CafeteriaName { get; set; } = null!;
        public MealType MealType { get; set; }
        public string MealTypeName => MealType.ToString();
        public DateTime Date { get; set; }
        public string QRCode { get; set; } = null!;
        public MealReservationStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime? UsedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
