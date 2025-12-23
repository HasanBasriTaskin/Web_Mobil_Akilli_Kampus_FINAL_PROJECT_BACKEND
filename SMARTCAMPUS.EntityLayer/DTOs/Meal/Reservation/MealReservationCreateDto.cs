using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.DTOs.Meal.Reservation
{
    public class MealReservationCreateDto
    {
        public int MenuId { get; set; }
        public int CafeteriaId { get; set; }
        public MealType MealType { get; set; }
        public DateTime Date { get; set; }
    }
}
