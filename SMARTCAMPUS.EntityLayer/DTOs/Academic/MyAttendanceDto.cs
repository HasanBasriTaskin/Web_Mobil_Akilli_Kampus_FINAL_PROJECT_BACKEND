namespace SMARTCAMPUS.EntityLayer.DTOs.Academic
{
    public class MyAttendanceDto
    {
        public List<CourseAttendanceDto> Courses { get; set; } = new();
        public decimal OverallAttendancePercentage { get; set; }
        public int TotalSessions { get; set; }
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
    }

    public class CourseAttendanceDto
    {
        public int SectionId { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? Semester { get; set; }
        public int Year { get; set; }
        public int TotalSessions { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public decimal AttendancePercentage { get; set; }
        public string Status { get; set; } = "Normal"; // "Normal", "Warning", "Critical"
    }
}

