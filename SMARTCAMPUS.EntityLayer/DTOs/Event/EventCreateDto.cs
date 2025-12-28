namespace SMARTCAMPUS.EntityLayer.DTOs.Event
{
    public class EventCreateDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int CategoryId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; } = null!;
        public int Capacity { get; set; }
        public decimal Price { get; set; } = 0;
        public string? ImageUrl { get; set; }
    }
}
