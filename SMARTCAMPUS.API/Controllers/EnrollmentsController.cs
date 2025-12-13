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
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly UserClaimsHelper _userClaimsHelper;

        public EnrollmentsController(IEnrollmentService enrollmentService, UserClaimsHelper userClaimsHelper)
        {
            _enrollmentService = enrollmentService;
            _userClaimsHelper = userClaimsHelper;
        }

        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll([FromBody] EnrollmentRequestDto request)
        {
            var studentId = await _userClaimsHelper.GetStudentIdAsync();
            if (!studentId.HasValue)
                return Unauthorized("Student not found or user is not a student");

            var result = await _enrollmentService.EnrollAsync(studentId.Value, request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("drop/{enrollmentId}")]
        public async Task<IActionResult> DropCourse(int enrollmentId)
        {
            var studentId = await _userClaimsHelper.GetStudentIdAsync();
            if (!studentId.HasValue)
                return Unauthorized("Student not found or user is not a student");

            var result = await _enrollmentService.DropCourseAsync(studentId.Value, enrollmentId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentEnrollments(int studentId)
        {
            var result = await _enrollmentService.GetStudentEnrollmentsAsync(studentId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("section/{sectionId}")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> GetSectionEnrollments(int sectionId)
        {
            var result = await _enrollmentService.GetSectionEnrollmentsAsync(sectionId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("check-prerequisites/{courseId}/{studentId}")]
        public async Task<IActionResult> CheckPrerequisites(int courseId, int studentId)
        {
            var result = await _enrollmentService.CheckPrerequisitesAsync(courseId, studentId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("check-conflict/{sectionId}/{studentId}")]
        public async Task<IActionResult> CheckScheduleConflict(int sectionId, int studentId)
        {
            var result = await _enrollmentService.CheckScheduleConflictAsync(studentId, sectionId);
            return StatusCode(result.StatusCode, result);
        }
    }
}



