namespace SMARTCAMPUS.EntityLayer.DTOs.Scheduling
{
    public class ScheduleDto
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string CourseCode { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public string SectionNumber { get; set; } = null!;
        public int ClassroomId { get; set; }
        public string ClassroomInfo { get; set; } = null!;
        public DayOfWeek DayOfWeek { get; set; }
        public string DayName => DayOfWeek.ToString();
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string TimeRange => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
        public string? InstructorName { get; set; }
    }
}
