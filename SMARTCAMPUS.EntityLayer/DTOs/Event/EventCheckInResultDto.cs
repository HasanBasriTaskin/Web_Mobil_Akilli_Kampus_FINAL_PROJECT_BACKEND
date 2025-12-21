namespace SMARTCAMPUS.EntityLayer.DTOs.Event
{
    public class EventCheckInResultDto
    {
        public int RegistrationId { get; set; }
        public string UserName { get; set; } = null!;
        public string EventTitle { get; set; } = null!;
        public bool IsValid { get; set; }
        public string? Message { get; set; }
    }
}
