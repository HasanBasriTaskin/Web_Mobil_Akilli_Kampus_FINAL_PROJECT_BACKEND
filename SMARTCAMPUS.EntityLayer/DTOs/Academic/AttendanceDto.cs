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
        public DateTime? QrCodeGeneratedAt { get; set; }
        public DateTime? QrCodeExpiresAt { get; set; }
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
        public string? IpAddress { get; set; }
        public bool IsMockLocation { get; set; }
        public decimal? Velocity { get; set; }
        public string? DeviceInfo { get; set; }
        public int FraudScore { get; set; }
        public bool IsFlagged { get; set; }
        public string? FlagReason { get; set; }
    }

    public class AttendanceCheckInDto
    {
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Accuracy { get; set; } // GPS accuracy in meters
        public bool IsMockLocation { get; set; } = false;
        public string? DeviceInfo { get; set; } // JSON: {"sensors": {...}, "browser": "..."}
        public string? QrCode { get; set; }
    }

    public class QrCodeRefreshDto
    {
        public string QrCode { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}



