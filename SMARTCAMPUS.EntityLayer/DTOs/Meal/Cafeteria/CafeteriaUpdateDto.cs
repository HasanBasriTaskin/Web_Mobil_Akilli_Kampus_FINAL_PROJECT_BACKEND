namespace SMARTCAMPUS.EntityLayer.DTOs.Meal.Cafeteria
{
    public class CafeteriaUpdateDto
    {
        public string? Name { get; set; }
        public string? Location { get; set; }
        public int? Capacity { get; set; }
        public bool? IsActive { get; set; }
    }
}
