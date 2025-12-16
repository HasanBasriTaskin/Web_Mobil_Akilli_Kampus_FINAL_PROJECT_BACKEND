using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Enrollment;
using System.Security.Claims;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentsController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        /// <summary>
        /// Enroll in a course section
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> EnrollInCourse([FromBody] CreateEnrollmentDto dto)
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null)
                return Unauthorized("Student ID not found");

            var result = await _enrollmentService.EnrollInCourseAsync(studentId.Value, dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Drop a course (withdraw)
        /// </summary>
        [HttpDelete("{enrollmentId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> DropCourse(int enrollmentId)
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null)
                return Unauthorized("Student ID not found");

            var result = await _enrollmentService.DropCourseAsync(studentId.Value, enrollmentId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get current student's enrolled courses
        /// </summary>
        [HttpGet("my-courses")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyCourses()
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null)
                return Unauthorized("Student ID not found");

            var result = await _enrollmentService.GetMyCoursesAsync(studentId.Value);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get students enrolled in a section (Faculty only)
        /// </summary>
        [HttpGet("sections/{sectionId}/students")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> GetStudentsBySection(int sectionId)
        {
            var instructorId = GetCurrentFacultyId();
            if (instructorId == null)
                return Unauthorized("Faculty ID not found");

            var result = await _enrollmentService.GetStudentsBySectionAsync(sectionId, instructorId.Value);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Check prerequisites for a course
        /// </summary>
        [HttpGet("check-prerequisites/{courseId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CheckPrerequisites(int courseId)
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null)
                return Unauthorized("Student ID not found");

            var result = await _enrollmentService.CheckPrerequisitesAsync(studentId.Value, courseId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Check schedule conflicts for a section
        /// </summary>
        [HttpGet("check-conflict/{sectionId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CheckScheduleConflict(int sectionId)
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null)
                return Unauthorized("Student ID not found");

            var result = await _enrollmentService.CheckScheduleConflictAsync(studentId.Value, sectionId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get faculty's own sections with pending count
        /// </summary>
        [HttpGet("my-sections")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> GetMySections()
        {
            var instructorId = GetCurrentFacultyId();
            if (instructorId == null)
                return Unauthorized("Faculty ID not found");

            var result = await _enrollmentService.GetMySectionsAsync(instructorId.Value);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Get pending enrollment requests for a section (Faculty only)
        /// </summary>
        [HttpGet("sections/{sectionId}/pending")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> GetPendingEnrollments(int sectionId)
        {
            var instructorId = GetCurrentFacultyId();
            if (instructorId == null)
                return Unauthorized("Faculty ID not found");

            var result = await _enrollmentService.GetPendingEnrollmentsAsync(sectionId, instructorId.Value);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Approve an enrollment request (Faculty only)
        /// </summary>
        [HttpPost("{enrollmentId}/approve")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> ApproveEnrollment(int enrollmentId)
        {
            var instructorId = GetCurrentFacultyId();
            if (instructorId == null)
                return Unauthorized("Faculty ID not found");

            var result = await _enrollmentService.ApproveEnrollmentAsync(enrollmentId, instructorId.Value);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Reject an enrollment request (Faculty only)
        /// </summary>
        [HttpPost("{enrollmentId}/reject")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> RejectEnrollment(int enrollmentId, [FromBody] RejectEnrollmentDto? dto)
        {
            var instructorId = GetCurrentFacultyId();
            if (instructorId == null)
                return Unauthorized("Faculty ID not found");

            var result = await _enrollmentService.RejectEnrollmentAsync(enrollmentId, instructorId.Value, dto?.Reason);
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
