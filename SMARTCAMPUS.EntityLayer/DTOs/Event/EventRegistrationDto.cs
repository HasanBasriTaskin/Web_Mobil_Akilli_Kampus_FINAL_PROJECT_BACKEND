namespace SMARTCAMPUS.EntityLayer.DTOs.Event
{
    public class EventRegistrationDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string EventTitle { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public DateTime RegistrationDate { get; set; }
        public string QRCode { get; set; } = null!;
        public bool CheckedIn { get; set; }
        public DateTime? CheckedInAt { get; set; }
    }
}
