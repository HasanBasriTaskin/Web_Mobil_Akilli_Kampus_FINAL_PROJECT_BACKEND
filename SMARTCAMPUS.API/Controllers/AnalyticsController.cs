using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;

namespace SMARTCAMPUS.API.Controllers
{
    /// <summary>
    /// Analitik ve raporlama endpoint'leri
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Admin dashboard için genel istatistikleri getirir
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var response = await _analyticsService.GetDashboardStatsAsync();
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Akademik performans raporunu getirir
        /// </summary>
        [HttpGet("academic-performance")]
        public async Task<IActionResult> GetAcademicPerformance()
        {
            var response = await _analyticsService.GetAcademicPerformanceAsync();
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Tüm bölümlerin GPA istatistiklerini getirir
        /// </summary>
        [HttpGet("department-stats")]
        public async Task<IActionResult> GetDepartmentGpaStats()
        {
            var response = await _analyticsService.GetDepartmentGpaStatsAsync();
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Belirli bir bölümün istatistiklerini getirir
        /// </summary>
        /// <param name="departmentId">Bölüm ID</param>
        [HttpGet("department/{departmentId}")]
        public async Task<IActionResult> GetDepartmentStats(int departmentId)
        {
            var response = await _analyticsService.GetDepartmentStatsAsync(departmentId);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Harf notu dağılımını getirir
        /// </summary>
        /// <param name="sectionId">Opsiyonel: Belirli bir ders için filtreleme</param>
        [HttpGet("grade-distribution")]
        public async Task<IActionResult> GetGradeDistribution([FromQuery] int? sectionId = null)
        {
            var response = await _analyticsService.GetGradeDistributionAsync(sectionId);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Riskli öğrencilerin listesini getirir
        /// </summary>
        /// <param name="gpaThreshold">GPA eşik değeri (varsayılan 2.0)</param>
        /// <param name="attendanceThreshold">Devamsızlık eşik değeri (varsayılan %20)</param>
        [HttpGet("at-risk-students")]
        public async Task<IActionResult> GetAtRiskStudents(
            [FromQuery] double gpaThreshold = 2.0,
            [FromQuery] double attendanceThreshold = 20)
        {
            var response = await _analyticsService.GetAtRiskStudentsAsync(gpaThreshold, attendanceThreshold);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Ders doluluk oranlarını getirir
        /// </summary>
        [HttpGet("course-occupancy")]
        public async Task<IActionResult> GetCourseOccupancy()
        {
            var response = await _analyticsService.GetCourseOccupancyAsync();
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Devamsızlık istatistiklerini getirir
        /// </summary>
        [HttpGet("attendance-stats")]
        public async Task<IActionResult> GetAttendanceStats()
        {
            var response = await _analyticsService.GetAttendanceStatsAsync();
            return StatusCode(response.StatusCode, response);
        }
    }
}
