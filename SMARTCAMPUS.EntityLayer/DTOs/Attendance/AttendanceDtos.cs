using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.DTOs.Attendance
{
    public class AttendanceSessionDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int GeofenceRadius { get; set; }
        public string? QRCode { get; set; }
        public AttendanceSessionStatus Status { get; set; }
        
        // Section Info
        public int SectionId { get; set; }
        public string CourseCode { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public string SectionNumber { get; set; } = null!;
        
        // Stats
        public int TotalStudents { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
    }

    public class CreateSessionDto
    {
        public int SectionId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int GeofenceRadius { get; set; } = 15;
    }

    public class CheckInDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Accuracy { get; set; }
    }

    public class CheckInResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public double DistanceFromCenter { get; set; }
        public bool IsFlagged { get; set; }
        public string? FlagReason { get; set; }
    }

    public class AttendanceRecordDto
    {
        public int Id { get; set; }
        public DateTime CheckInTime { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double DistanceFromCenter { get; set; }
        public bool IsFlagged { get; set; }
        public string? FlagReason { get; set; }
        
        // Student Info
        public int StudentId { get; set; }
        public string StudentNumber { get; set; } = null!;
        public string StudentName { get; set; } = null!;
    }

    public class StudentAttendanceDto
    {
        public string CourseCode { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public int TotalSessions { get; set; }
        public int AttendedSessions { get; set; }
        public int ExcusedSessions { get; set; }
        public double AttendancePercentage { get; set; }
        public string WarningLevel { get; set; } = null!; // "OK", "Warning", "Critical"
    }
}
