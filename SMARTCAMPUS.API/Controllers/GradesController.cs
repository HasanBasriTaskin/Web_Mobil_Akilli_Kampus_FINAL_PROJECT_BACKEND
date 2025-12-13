using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

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



