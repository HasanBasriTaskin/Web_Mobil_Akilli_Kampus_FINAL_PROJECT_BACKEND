namespace SMARTCAMPUS.EntityLayer.DTOs.Scheduling
{
    public class TimeSlotDto
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string TimeRange => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
        public string? ReservedBy { get; set; }
        public string? Purpose { get; set; }
    }
}
