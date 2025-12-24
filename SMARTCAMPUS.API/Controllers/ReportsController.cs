using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;

namespace SMARTCAMPUS.API.Controllers
{
    /// <summary>
    /// Rapor dışa aktarma endpoint'leri (Excel ve PDF)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportExportService _reportExportService;

        public ReportsController(IReportExportService reportExportService)
        {
            _reportExportService = reportExportService;
        }

        /// <summary>
        /// Öğrenci listesini Excel olarak indirir
        /// </summary>
        /// <param name="departmentId">Opsiyonel: Bölüm filtresi</param>
        [HttpGet("students/excel")]
        [Authorize(Roles = "Admin,Faculty")]
        public async Task<IActionResult> ExportStudentListToExcel([FromQuery] int? departmentId = null)
        {
            try
            {
                var bytes = await _reportExportService.ExportStudentListToExcelAsync(departmentId);
                return File(bytes, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"ogrenci_listesi_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Ders not raporunu Excel olarak indirir
        /// </summary>
        /// <param name="sectionId">Ders Section ID</param>
        [HttpGet("grades/{sectionId}/excel")]
        [Authorize(Roles = "Admin,Faculty")]
        public async Task<IActionResult> ExportGradeReportToExcel(int sectionId)
        {
            try
            {
                var bytes = await _reportExportService.ExportGradeReportToExcelAsync(sectionId);
                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"not_raporu_{sectionId}_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Öğrenci transkriptini PDF olarak indirir
        /// </summary>
        /// <param name="studentId">Öğrenci ID</param>
        [HttpGet("transcript/{studentId}/pdf")]
        [Authorize(Roles = "Admin,Faculty,Student")]
        public async Task<IActionResult> ExportTranscriptToPdf(int studentId)
        {
            try
            {
                var bytes = await _reportExportService.ExportTranscriptToPdfAsync(studentId);
                return File(bytes,
                    "application/pdf",
                    $"transkript_{studentId}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Ders devamsızlık raporunu PDF olarak indirir
        /// </summary>
        /// <param name="sectionId">Ders Section ID</param>
        [HttpGet("attendance/{sectionId}/pdf")]
        [Authorize(Roles = "Admin,Faculty")]
        public async Task<IActionResult> ExportAttendanceReportToPdf(int sectionId)
        {
            try
            {
                var bytes = await _reportExportService.ExportAttendanceReportToPdfAsync(sectionId);
                return File(bytes,
                    "application/pdf",
                    $"devamsizlik_raporu_{sectionId}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Riskli öğrenciler raporunu Excel olarak indirir
        /// </summary>
        /// <param name="gpaThreshold">GPA eşik değeri (varsayılan 2.0)</param>
        [HttpGet("at-risk-students/excel")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportAtRiskStudentsToExcel([FromQuery] double gpaThreshold = 2.0)
        {
            try
            {
                var bytes = await _reportExportService.ExportAtRiskStudentsToExcelAsync(gpaThreshold);
                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"riskli_ogrenciler_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
