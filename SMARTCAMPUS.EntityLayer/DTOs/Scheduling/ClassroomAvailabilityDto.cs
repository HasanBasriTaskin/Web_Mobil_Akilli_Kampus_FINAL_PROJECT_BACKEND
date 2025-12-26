namespace SMARTCAMPUS.EntityLayer.DTOs.Scheduling
{
    public class ClassroomAvailabilityDto
    {
        public int ClassroomId { get; set; }
        public string ClassroomInfo { get; set; } = null!;
        public DateTime Date { get; set; }
        public List<TimeSlotDto> AvailableSlots { get; set; } = new();
        public List<TimeSlotDto> ReservedSlots { get; set; } = new();
    }
}
