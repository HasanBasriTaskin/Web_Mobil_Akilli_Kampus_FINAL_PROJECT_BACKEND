using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs.Analytics;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    /// <summary>
    /// Analitik ve raporlama servisi
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Admin dashboard için genel istatistikleri getirir
        /// </summary>
        Task<Response<AdminDashboardDto>> GetDashboardStatsAsync();

        /// <summary>
        /// Akademik performans raporunu getirir
        /// </summary>
        Task<Response<AcademicPerformanceDto>> GetAcademicPerformanceAsync();

        /// <summary>
        /// Bölüm bazlı GPA istatistiklerini getirir
        /// </summary>
        Task<Response<List<DepartmentGpaDto>>> GetDepartmentGpaStatsAsync();

        /// <summary>
        /// Belirli bir bölümün istatistiklerini getirir
        /// </summary>
        Task<Response<DepartmentGpaDto>> GetDepartmentStatsAsync(int departmentId);

        /// <summary>
        /// Harf notu dağılımını getirir
        /// </summary>
        Task<Response<List<GradeDistributionDto>>> GetGradeDistributionAsync(int? sectionId = null);

        /// <summary>
        /// Riskli öğrencilerin listesini getirir
        /// </summary>
        /// <param name="gpaThreshold">GPA eşik değeri (varsayılan 2.0)</param>
        /// <param name="attendanceThreshold">Devamsızlık eşik değeri (varsayılan %20)</param>
        Task<Response<List<AtRiskStudentDto>>> GetAtRiskStudentsAsync(double gpaThreshold = 2.0, double attendanceThreshold = 20);

        /// <summary>
        /// Ders doluluk oranlarını getirir
        /// </summary>
        Task<Response<List<CourseOccupancyDto>>> GetCourseOccupancyAsync();

        /// <summary>
        /// Devamsızlık istatistiklerini getirir
        /// </summary>
        Task<Response<AttendanceStatsDto>> GetAttendanceStatsAsync();
    }
}
