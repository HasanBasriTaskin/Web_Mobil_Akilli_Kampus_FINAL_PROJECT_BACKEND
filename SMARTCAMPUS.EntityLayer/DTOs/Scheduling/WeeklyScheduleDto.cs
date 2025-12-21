namespace SMARTCAMPUS.EntityLayer.DTOs.Scheduling
{
    public class WeeklyScheduleDto
    {
        public DayOfWeek Day { get; set; }
        public string DayName => Day.ToString();
        public List<ScheduleDto> Schedules { get; set; } = new();
    }
}
