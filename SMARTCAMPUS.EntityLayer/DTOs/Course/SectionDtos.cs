namespace SMARTCAMPUS.EntityLayer.DTOs.Course
{
    public class CourseSectionDto
    {
        public int Id { get; set; }
        public string SectionNumber { get; set; } = null!;
        public string Semester { get; set; } = null!;
        public int Year { get; set; }
        public int Capacity { get; set; }
        public int EnrolledCount { get; set; }
        public int AvailableSeats => Capacity - EnrolledCount;
        public string? ScheduleJson { get; set; }
        
        // Course Info
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        
        // Instructor Info
        public int InstructorId { get; set; }
        public string InstructorName { get; set; } = null!;
        public string InstructorTitle { get; set; } = null!;
    }

    public class CreateSectionDto
    {
        public string SectionNumber { get; set; } = null!;
        public string Semester { get; set; } = null!;
        public int Year { get; set; }
        public int Capacity { get; set; }
        public string? ScheduleJson { get; set; }
        public int CourseId { get; set; }
        public int InstructorId { get; set; }
    }

    public class UpdateSectionDto
    {
        public int? Capacity { get; set; }
        public string? ScheduleJson { get; set; }
        public int? InstructorId { get; set; }
    }

    public class SectionListDto
    {
        public int Id { get; set; }
        public string SectionNumber { get; set; } = null!;
        public string CourseCode { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public string InstructorName { get; set; } = null!;
        public int AvailableSeats { get; set; }
        public string Semester { get; set; } = null!;
        public int Year { get; set; }
    }
}
