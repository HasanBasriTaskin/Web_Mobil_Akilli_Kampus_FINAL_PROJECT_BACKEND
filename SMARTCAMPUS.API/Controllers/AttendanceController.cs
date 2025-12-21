using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Attendance;
using System.Security.Claims;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        #region Session Management (Faculty)

        [HttpPost("sessions")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionDto dto)
        {
            var instructorId = GetCurrentFacultyId();
            if (instructorId == null)
                return Unauthorized("Faculty ID not found");

            var result = await _attendanceService.CreateSessionAsync(instructorId.Value, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("sessions/{sessionId}")]
        public async Task<IActionResult> GetSession(int sessionId)
        {
            var result = await _attendanceService.GetSessionByIdAsync(sessionId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("sessions/{sessionId}/close")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> CloseSession(int sessionId)
        {
            var instructorId = GetCurrentFacultyId();
            if (instructorId == null)
                return Unauthorized("Faculty ID not found");

            var result = await _attendanceService.CloseSessionAsync(instructorId.Value, sessionId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("sessions/my-sessions")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> GetMySessions()
        {
            var instructorId = GetCurrentFacultyId();
            if (instructorId == null)
                return Unauthorized("Faculty ID not found");

            var result = await _attendanceService.GetMySessionsAsync(instructorId.Value);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("sessions/{sessionId}/records")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> GetSessionRecords(int sessionId)
        {
            var result = await _attendanceService.GetSessionRecordsAsync(sessionId);
            return StatusCode(result.StatusCode, result);
        }

        #endregion

        #region Student Check-in

        [HttpPost("sessions/{sessionId}/checkin")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CheckIn(int sessionId, [FromBody] CheckInDto dto)
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null)
                return Unauthorized("Student ID not found");

            var result = await _attendanceService.CheckInAsync(studentId.Value, sessionId, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("my-attendance")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyAttendance()
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null)
                return Unauthorized("Student ID not found");

            var result = await _attendanceService.GetMyAttendanceAsync(studentId.Value);
            return StatusCode(result.StatusCode, result);
        }

        #endregion

        #region Excuse Requests

        [HttpPost("excuse-requests")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CreateExcuseRequest([FromBody] CreateExcuseRequestDto dto)
        {
            var studentId = GetCurrentStudentId();
            if (studentId == null)
                return Unauthorized("Student ID not found");

            // TODO: Handle file upload for document
            var result = await _attendanceService.CreateExcuseRequestAsync(studentId.Value, dto, null);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("excuse-requests")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> GetExcuseRequests([FromQuery] int? sectionId = null)
        {
            var instructorId = GetCurrentFacultyId();
            if (instructorId == null)
                return Unauthorized("Faculty ID not found");

            var result = await _attendanceService.GetExcuseRequestsAsync(instructorId.Value, sectionId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("excuse-requests/{requestId}/approve")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> ApproveExcuseRequest(int requestId, [FromBody] ReviewExcuseRequestDto dto)
        {
            var instructorId = GetCurrentFacultyId();
            if (instructorId == null)
                return Unauthorized("Faculty ID not found");

            var result = await _attendanceService.ApproveExcuseRequestAsync(instructorId.Value, requestId, dto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("excuse-requests/{requestId}/reject")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> RejectExcuseRequest(int requestId, [FromBody] ReviewExcuseRequestDto dto)
        {
            var instructorId = GetCurrentFacultyId();
            if (instructorId == null)
                return Unauthorized("Faculty ID not found");

            var result = await _attendanceService.RejectExcuseRequestAsync(instructorId.Value, requestId, dto);
            return StatusCode(result.StatusCode, result);
        }

        #endregion

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
