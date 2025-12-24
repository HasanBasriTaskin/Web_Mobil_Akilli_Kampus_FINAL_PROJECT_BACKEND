namespace SMARTCAMPUS.EntityLayer.DTOs.Analytics
{
    /// <summary>
    /// Admin Dashboard için sistem istatistikleri
    /// </summary>
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalFaculty { get; set; }
        public int DailyActiveUsers { get; set; }
        public int TotalCourses { get; set; }
        public int TotalCourseSections { get; set; }
        public int ActiveEnrollments { get; set; }
        public double AverageClassOccupancy { get; set; }
        public int TotalEvents { get; set; }
        public int UpcomingEvents { get; set; }
        public SystemHealthDto SystemHealth { get; set; } = new();
    }

    /// <summary>
    /// Sistem sağlık durumu
    /// </summary>
    public class SystemHealthDto
    {
        public string DatabaseStatus { get; set; } = "Healthy";
        public string ApiStatus { get; set; } = "Running";
        public DateTime LastChecked { get; set; } = DateTime.UtcNow;
        public int ActiveConnections { get; set; }
        public double CpuUsagePercent { get; set; }
        public double MemoryUsagePercent { get; set; }
    }

    /// <summary>
    /// Bölüm bazlı GPA istatistikleri
    /// </summary>
    public class DepartmentGpaDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string DepartmentCode { get; set; } = string.Empty;
        public double AverageGpa { get; set; }
        public double AverageCgpa { get; set; }
        public int StudentCount { get; set; }
    }

    /// <summary>
    /// Harf notu dağılımı
    /// </summary>
    public class GradeDistributionDto
    {
        public string LetterGrade { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// Riskli öğrenci bilgisi
    /// </summary>
    public class AtRiskStudentDto
    {
        public int StudentId { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public double Gpa { get; set; }
        public double Cgpa { get; set; }
        public double AttendanceRate { get; set; }
        public string RiskLevel { get; set; } = string.Empty; // Low, Medium, High, Critical
        public List<string> RiskFactors { get; set; } = new();
    }

    /// <summary>
    /// Akademik performans özeti
    /// </summary>
    public class AcademicPerformanceDto
    {
        public double OverallAverageGpa { get; set; }
        public int TotalEnrollments { get; set; }
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public double PassRate { get; set; }
        public List<DepartmentGpaDto> DepartmentStats { get; set; } = new();
        public List<GradeDistributionDto> GradeDistribution { get; set; } = new();
    }

    /// <summary>
    /// Ders doluluk istatistikleri
    /// </summary>
    public class CourseOccupancyDto
    {
        public int SectionId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string SectionCode { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int EnrolledCount { get; set; }
        public double OccupancyRate { get; set; }
    }

    /// <summary>
    /// Devamsızlık istatistikleri
    /// </summary>
    public class AttendanceStatsDto
    {
        public int TotalSessions { get; set; }
        public int TotalAttendanceRecords { get; set; }
        public double OverallAttendanceRate { get; set; }
        public int StudentsAboveThreshold { get; set; } // %20+ devamsızlık
        public List<SectionAttendanceDto> SectionStats { get; set; } = new();
    }

    /// <summary>
    /// Ders başına devamsızlık
    /// </summary>
    public class SectionAttendanceDto
    {
        public int SectionId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int TotalSessions { get; set; }
        public double AverageAttendanceRate { get; set; }
    }
}
