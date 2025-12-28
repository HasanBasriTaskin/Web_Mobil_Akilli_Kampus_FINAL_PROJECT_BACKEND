using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.DTOs.Meal.Menu
{
    public class MealMenuCreateDto
    {
        public int CafeteriaId { get; set; }
        public DateTime Date { get; set; }
        public MealType MealType { get; set; }
        public decimal Price { get; set; }
        public bool IsPublished { get; set; } = false;
        public List<int> FoodItemIds { get; set; } = new();
        public MealNutritionCreateDto? Nutrition { get; set; }
    }
}
