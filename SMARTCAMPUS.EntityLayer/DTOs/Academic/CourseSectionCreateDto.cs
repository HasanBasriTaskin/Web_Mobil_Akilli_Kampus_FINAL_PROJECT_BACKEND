namespace SMARTCAMPUS.EntityLayer.DTOs.Academic
{
    public class CourseSectionCreateDto
    {
        public int CourseId { get; set; }
        public string SectionNumber { get; set; } = null!;
        public string Semester { get; set; } = null!;
        public int Year { get; set; }
        public string? InstructorId { get; set; }
        public int Capacity { get; set; }
        public string? ScheduleJson { get; set; }
        public int? ClassroomId { get; set; }
    }
}

