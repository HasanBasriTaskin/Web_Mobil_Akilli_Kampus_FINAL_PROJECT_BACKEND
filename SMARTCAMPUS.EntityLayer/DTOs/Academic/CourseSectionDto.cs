namespace SMARTCAMPUS.EntityLayer.DTOs.Academic
{
    public class CourseSectionDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string SectionNumber { get; set; } = null!;
        public string Semester { get; set; } = null!;
        public int Year { get; set; }
        public string? InstructorId { get; set; }
        public string? InstructorName { get; set; }
        public int Capacity { get; set; }
        public int EnrolledCount { get; set; }
        public string? ScheduleJson { get; set; }
        public int? ClassroomId { get; set; }
        public string? ClassroomInfo { get; set; } // "Building-RoomNumber"
        public bool IsFull => EnrolledCount >= Capacity;
    }
}

