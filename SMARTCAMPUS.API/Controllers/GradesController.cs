using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Tools;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class GradesController : ControllerBase
    {
        private readonly IGradeService _gradeService;
        private readonly ITranscriptService _transcriptService;
        private readonly UserClaimsHelper _userClaimsHelper;

        public GradesController(IGradeService gradeService, ITranscriptService transcriptService, UserClaimsHelper userClaimsHelper)
        {
            _gradeService = gradeService;
            _transcriptService = transcriptService;
            _userClaimsHelper = userClaimsHelper;
        }

        [HttpGet("section/{sectionId}")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> GetSectionGrades(int sectionId)
        {
            var result = await _gradeService.GetSectionGradesAsync(sectionId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("enrollment/{enrollmentId}")]
        public async Task<IActionResult> GetStudentGrade(int enrollmentId)
        {
            var result = await _gradeService.GetStudentGradeAsync(enrollmentId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("enrollment/{enrollmentId}")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> UpdateGrade(int enrollmentId, [FromBody] GradeUpdateDto gradeUpdate)
        {
            var result = await _gradeService.UpdateGradeAsync(enrollmentId, gradeUpdate);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("section/{sectionId}/bulk")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> BulkUpdateGrades(int sectionId, [FromBody] GradeBulkUpdateDto grades)
        {
            var result = await _gradeService.BulkUpdateGradesAsync(sectionId, grades);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("my-grades")]
        public async Task<IActionResult> GetMyGrades()
        {
            var studentId = await _userClaimsHelper.GetStudentIdAsync();
            if (!studentId.HasValue)
                return Unauthorized("Student not found or user is not a student");

            var result = await _gradeService.GetMyGradesAsync(studentId.Value);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("transcript")]
        public async Task<IActionResult> GetTranscript()
        {
            var studentId = await _userClaimsHelper.GetStudentIdAsync();
            if (!studentId.HasValue)
                return Unauthorized("Student not found or user is not a student");

            var result = await _transcriptService.GetTranscriptAsync(studentId.Value);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("transcript/pdf")]
        public async Task<IActionResult> GetTranscriptPdf()
        {
            var studentId = await _userClaimsHelper.GetStudentIdAsync();
            if (!studentId.HasValue)
                return Unauthorized("Student not found or user is not a student");

            var result = await _transcriptService.GenerateTranscriptPdfAsync(studentId.Value);
            if (!result.IsSuccessful || result.Data == null)
                return StatusCode(result.StatusCode, result);

            return File(result.Data, "application/pdf", $"transcript_{studentId}_{DateTime.UtcNow:yyyyMMdd}.pdf");
        }

        [HttpPost]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> CreateGrade([FromBody] GradeUpdateDto gradeUpdate)
        {
            var instructorId = _userClaimsHelper.GetUserId();
            if (string.IsNullOrEmpty(instructorId))
                return Unauthorized("Instructor not found");

            var result = await _gradeService.CreateGradeAsync(gradeUpdate.EnrollmentId, gradeUpdate, instructorId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("calculate-letter")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> CalculateLetterGrade([FromBody] GradeCalculationDto calculation)
        {
            var result = await _gradeService.CalculateLetterGradeAsync(calculation.MidtermGrade, calculation.FinalGrade);
            return StatusCode(result.StatusCode, result);
        }
    }

    public class GradeCalculationDto
    {
        public decimal? MidtermGrade { get; set; }
        public decimal? FinalGrade { get; set; }
    }
}



