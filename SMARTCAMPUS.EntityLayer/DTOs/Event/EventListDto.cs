namespace SMARTCAMPUS.EntityLayer.DTOs.Event
{
    public class EventListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; } = null!;
        public int AvailableSpots { get; set; }
        public bool IsFull { get; set; }
        public decimal Price { get; set; }
        public bool IsFree => Price == 0;
        public string? ImageUrl { get; set; }
    }
}
