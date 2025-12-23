namespace SMARTCAMPUS.EntityLayer.DTOs.Event
{
    public class EventFilterDto
    {
        public int? CategoryId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsFree { get; set; }
        public bool? HasAvailableSpots { get; set; }
        public string? Search { get; set; }
    }
}
