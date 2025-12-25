namespace SMARTCAMPUS.EntityLayer.DTOs.Event
{
    public class EventUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Location { get; set; }
        public int? Capacity { get; set; }
        public decimal? Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsActive { get; set; }
    }
}
