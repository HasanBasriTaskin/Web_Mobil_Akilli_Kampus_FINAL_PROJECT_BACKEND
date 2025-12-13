namespace SMARTCAMPUS.EntityLayer.Models
{
    public class AcademicCalendar : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string EventType { get; set; } = null!; // "Holiday", "Exam", "Registration", "Semester Start", etc.
        public bool IsHoliday { get; set; }
        public int? Year { get; set; }
        public string? Semester { get; set; } // "Fall", "Spring", "Summer"
    }
}

