namespace SMARTCAMPUS.EntityLayer.DTOs.Academic
{
    public class AcademicCalendarDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string EventType { get; set; } = null!;
        public bool IsHoliday { get; set; }
        public int? Year { get; set; }
        public string? Semester { get; set; }
    }
}

