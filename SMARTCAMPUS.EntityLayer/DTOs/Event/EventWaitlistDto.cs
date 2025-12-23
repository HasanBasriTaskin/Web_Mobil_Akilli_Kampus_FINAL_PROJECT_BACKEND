namespace SMARTCAMPUS.EntityLayer.DTOs.Event
{
    public class EventWaitlistDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public int QueuePosition { get; set; }
        public DateTime AddedAt { get; set; }
        public bool IsNotified { get; set; }
    }
}
