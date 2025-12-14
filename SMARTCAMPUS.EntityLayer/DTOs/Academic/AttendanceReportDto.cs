namespace SMARTCAMPUS.EntityLayer.DTOs.Academic
{
    public class AttendanceReportDto
    {
        public int SectionId { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? Semester { get; set; }
        public int Year { get; set; }
        public int TotalSessions { get; set; }
        public int TotalStudents { get; set; }
        public List<StudentAttendanceSummaryDto> StudentSummaries { get; set; } = new();
    }

    public class StudentAttendanceSummaryDto
    {
        public int StudentId { get; set; }
        public string? StudentNumber { get; set; }
        public string? StudentName { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public decimal AttendancePercentage { get; set; }
    }
}

