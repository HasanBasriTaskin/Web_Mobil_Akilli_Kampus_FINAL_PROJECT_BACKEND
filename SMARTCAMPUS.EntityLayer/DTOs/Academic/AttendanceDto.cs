namespace SMARTCAMPUS.EntityLayer.DTOs.Academic
{
    public class AttendanceSessionDto
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? InstructorId { get; set; }
        public string? InstructorName { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? GeofenceRadius { get; set; }
        public string? QrCode { get; set; }
        public string Status { get; set; } = null!;
        public int TotalStudents { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
    }

    public class AttendanceRecordDto
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int StudentId { get; set; }
        public string? StudentNumber { get; set; }
        public string? StudentName { get; set; }
        public DateTime? CheckInTime { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? DistanceFromCenter { get; set; }
        public bool IsFlagged { get; set; }
        public string? FlagReason { get; set; }
    }

    public class AttendanceCheckInDto
    {
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Accuracy { get; set; } // GPS accuracy in meters
        public string? QrCode { get; set; }
    }
}



