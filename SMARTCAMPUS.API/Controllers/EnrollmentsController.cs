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

        [HttpPost]
        public async Task<IActionResult> Enroll([FromBody] EnrollmentRequestDto request)
        {
            var studentId = await _userClaimsHelper.GetStudentIdAsync();
            if (!studentId.HasValue)
                return Unauthorized("Student not found or user is not a student");

            var result = await _enrollmentService.EnrollAsync(studentId.Value, request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DropCourse(int id)
        {
            var studentId = await _userClaimsHelper.GetStudentIdAsync();
            if (!studentId.HasValue)
                return Unauthorized("Student not found or user is not a student");

            var result = await _enrollmentService.DropCourseAsync(studentId.Value, id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var studentId = await _userClaimsHelper.GetStudentIdAsync();
            if (!studentId.HasValue)
                return Unauthorized("Student not found or user is not a student");

            var isAdmin = User.IsInRole("Admin");
            var instructorId = User.IsInRole("Faculty") ? _userClaimsHelper.GetUserId() : null;
            
            var result = await _enrollmentService.GetStudentEnrollmentsAsync(studentId.Value, studentId, isAdmin, instructorId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("students/{sectionId}")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> GetSectionEnrollments(int sectionId)
        {
            var instructorId = _userClaimsHelper.GetUserId();
            var isAdmin = User.IsInRole("Admin");
            
            var result = await _enrollmentService.GetSectionEnrollmentsAsync(sectionId, instructorId, isAdmin);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("check-prerequisites/{courseId}/{studentId}")]
        public async Task<IActionResult> CheckPrerequisites(int courseId, int studentId)
        {
            // Authorization: Students can only check their own prerequisites
            // Faculty/Admin can check any student's prerequisites
            var currentStudentId = await _userClaimsHelper.GetStudentIdAsync();
            var isAdmin = User.IsInRole("Admin");
            var isFaculty = User.IsInRole("Faculty");
            
            if (!isAdmin && !isFaculty && (!currentStudentId.HasValue || currentStudentId.Value != studentId))
            {
                return Unauthorized("You can only check your own prerequisites");
            }

            var result = await _enrollmentService.CheckPrerequisitesAsync(courseId, studentId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("check-conflict/{sectionId}/{studentId}")]
        public async Task<IActionResult> CheckScheduleConflict(int sectionId, int studentId)
        {
            // Authorization: Students can only check their own schedule conflicts
            // Faculty/Admin can check any student's schedule conflicts
            var currentStudentId = await _userClaimsHelper.GetStudentIdAsync();
            var isAdmin = User.IsInRole("Admin");
            var isFaculty = User.IsInRole("Faculty");
            
            if (!isAdmin && !isFaculty && (!currentStudentId.HasValue || currentStudentId.Value != studentId))
            {
                return Unauthorized("You can only check your own schedule conflicts");
            }

            var result = await _enrollmentService.CheckScheduleConflictAsync(studentId, sectionId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("my-schedule")]
        public async Task<IActionResult> GetPersonalSchedule([FromQuery] string? semester, [FromQuery] int? year)
        {
            var studentId = await _userClaimsHelper.GetStudentIdAsync();
            if (!studentId.HasValue)
                return Unauthorized("Student not found or user is not a student");

            var result = await _enrollmentService.GetPersonalScheduleAsync(studentId.Value, semester, year);
            return StatusCode(result.StatusCode, result);
        }
    }
}



