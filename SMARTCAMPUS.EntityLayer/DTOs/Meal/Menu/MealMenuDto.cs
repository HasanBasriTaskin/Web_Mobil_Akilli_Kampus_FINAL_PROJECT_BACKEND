using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.DTOs.Meal.Menu
{
    public class MealMenuDto
    {
        public int Id { get; set; }
        public int CafeteriaId { get; set; }
        public string CafeteriaName { get; set; } = null!;
        public DateTime Date { get; set; }
        public MealType MealType { get; set; }
        public string MealTypeName => MealType.ToString();
        public decimal Price { get; set; }
        public bool IsPublished { get; set; }
        public List<MealMenuItemDto> MenuItems { get; set; } = new();
        public MealNutritionDto? Nutrition { get; set; }
    }
}
