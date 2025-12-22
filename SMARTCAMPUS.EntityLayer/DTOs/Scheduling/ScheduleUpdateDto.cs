namespace SMARTCAMPUS.EntityLayer.DTOs.Scheduling
{
    public class ScheduleUpdateDto
    {
        public int? SectionId { get; set; }
        public int? ClassroomId { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }
}
