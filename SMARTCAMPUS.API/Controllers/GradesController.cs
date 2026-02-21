using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Grade;
using System.Security.Claims;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class GradesController : ControllerBase
    {
        private readonly IGradeService _gradeService;

        public GradesController(IGradeService gradeService)
        {
            _gradeService = gradeService;
        }

        /// <summary>
        /// Get current student's grades
        /// </summary>
        [HttpGet("my-grades")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyGrades()
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null)
                return Unauthorized("Student ID not found");

            var result = await _gradeService.GetMyGradesAsync(studentId.Value);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get student transcript
        /// </summary>
        [HttpGet("transcript")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetTranscript()
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null)
                return Unauthorized("Student ID not found");

            var result = await _gradeService.GetTranscriptAsync(studentId.Value);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get transcript as PDF
        /// </summary>
        [HttpGet("transcript/pdf")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetTranscriptPdf()
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null)
                return Unauthorized("Student ID not found");

            var pdfBytes = await _gradeService.GenerateTranscriptPdfAsync(studentId.Value);
            return File(pdfBytes, "application/pdf", "transcript.pdf");
        }

        /// <summary>
        /// Enter a single grade (Faculty only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> EnterGrade([FromBody] GradeEntryDto dto)
        {
            try
            {
                var instructorId = GetCurrentFacultyId();
                if (instructorId == null)
                    return Unauthorized(new { success = false, message = "Faculty ID not found in token" });

                var result = await _gradeService.EnterGradeAsync(instructorId.Value, dto);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        /// <summary>
        /// Enter multiple grades at once (Faculty only)
        /// </summary>
        [HttpPost("batch")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> EnterGradesBatch([FromBody] List<GradeEntryDto> dtos)
        {
            var instructorId = GetCurrentFacultyId();
            if (instructorId == null)
                return Unauthorized("Faculty ID not found");

            var result = await _gradeService.EnterGradesBatchAsync(instructorId.Value, dtos);
            return StatusCode(result.StatusCode, result);
        }

        private int? GetCurrentStudentId()
        {
            var claim = User.FindFirst("StudentId");
            if (claim != null && int.TryParse(claim.Value, out var id))
                return id;
            return null;
        }

        private int? GetCurrentFacultyId()
        {
            var claim = User.FindFirst("FacultyId");
            if (claim != null && int.TryParse(claim.Value, out var id))
                return id;
            return null;
        }
    }
}
