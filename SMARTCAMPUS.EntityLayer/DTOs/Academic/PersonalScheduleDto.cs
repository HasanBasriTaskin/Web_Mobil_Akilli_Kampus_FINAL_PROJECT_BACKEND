using System.Text.Json.Serialization;

namespace SMARTCAMPUS.EntityLayer.DTOs.Academic
{
    public class PersonalScheduleDto
    {
        public string Semester { get; set; } = null!;
        public int Year { get; set; }
        public List<ScheduleItemDto> ScheduleItems { get; set; } = new();
    }

    public class ScheduleItemDto
    {
        public int SectionId { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? SectionNumber { get; set; }
        public string? InstructorName { get; set; }
        public string? ClassroomInfo { get; set; }
        public string Day { get; set; } = null!; // "Monday", "Tuesday", etc.
        public string StartTime { get; set; } = null!; // "09:00"
        public string EndTime { get; set; } = null!; // "10:30"
        public int? ClassroomId { get; set; }
    }
}
