namespace SMARTCAMPUS.EntityLayer.DTOs.Academic
{
    public class CourseSectionUpdateDto
    {
        public string? SectionNumber { get; set; }
        public string? Semester { get; set; }
        public int? Year { get; set; }
        public string? InstructorId { get; set; }
        public int? Capacity { get; set; }
        public string? ScheduleJson { get; set; }
        public int? ClassroomId { get; set; }
    }
}

