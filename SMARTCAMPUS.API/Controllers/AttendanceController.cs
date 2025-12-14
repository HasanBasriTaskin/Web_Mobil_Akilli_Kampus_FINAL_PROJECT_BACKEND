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
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        private readonly UserClaimsHelper _userClaimsHelper;

        public AttendanceController(IAttendanceService attendanceService, UserClaimsHelper userClaimsHelper)
        {
            _attendanceService = attendanceService;
            _userClaimsHelper = userClaimsHelper;
        }

        [HttpPost("sessions")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> CreateSession([FromBody] AttendanceSessionCreateDto sessionCreateDto)
        {
            var result = await _attendanceService.CreateSessionAsync(sessionCreateDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("sessions/{sessionId}/checkin")]
        public async Task<IActionResult> CheckIn(int sessionId, [FromBody] AttendanceCheckInDto checkInDto)
        {
            var studentId = await _userClaimsHelper.GetStudentIdAsync();
            if (!studentId.HasValue)
                return Unauthorized("Student not found or user is not a student");

            var result = await _attendanceService.CheckInAsync(studentId.Value, sessionId, checkInDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("sessions/{sessionId}")]
        public async Task<IActionResult> GetSession(int sessionId)
        {
            var studentId = await _userClaimsHelper.GetStudentIdAsync();
            var instructorId = User.IsInRole("Faculty") ? _userClaimsHelper.GetUserId() : null;
            var isAdmin = User.IsInRole("Admin");
            
            var result = await _attendanceService.GetSessionByIdAsync(sessionId, studentId, instructorId, isAdmin);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("sessions/{sessionId}/records")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> GetSessionRecords(int sessionId)
        {
            var instructorId = _userClaimsHelper.GetUserId();
            var isAdmin = User.IsInRole("Admin");
            
            var result = await _attendanceService.GetSessionRecordsAsync(sessionId, instructorId, isAdmin);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("students/{studentId}")]
        public async Task<IActionResult> GetStudentAttendance(int studentId)
        {
            var requestingStudentId = await _userClaimsHelper.GetStudentIdAsync();
            var instructorId = User.IsInRole("Faculty") ? _userClaimsHelper.GetUserId() : null;
            var isAdmin = User.IsInRole("Admin");
            
            var result = await _attendanceService.GetStudentAttendanceAsync(studentId, requestingStudentId, isAdmin, instructorId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("sessions/{id}/close")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> CloseSession(int id)
        {
            var instructorId = _userClaimsHelper.GetUserId();
            if (string.IsNullOrEmpty(instructorId))
                return Unauthorized("Instructor not found");

            var result = await _attendanceService.CloseSessionAsync(id, instructorId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("sessions/my-sessions")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> GetMySessions()
        {
            var instructorId = _userClaimsHelper.GetUserId();
            if (string.IsNullOrEmpty(instructorId))
                return Unauthorized("Instructor not found");

            var result = await _attendanceService.GetMySessionsAsync(instructorId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("report/{sectionId}")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> GetAttendanceReport(int sectionId)
        {
            var instructorId = _userClaimsHelper.GetUserId();
            var isAdmin = User.IsInRole("Admin");
            
            var result = await _attendanceService.GetSectionAttendanceReportAsync(sectionId, instructorId, isAdmin);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("sessions/{id}/qr-code")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> RefreshQrCode(int id)
        {
            var instructorId = _userClaimsHelper.GetUserId();
            var isAdmin = User.IsInRole("Admin");
            
            var result = await _attendanceService.RefreshQrCodeAsync(id, instructorId, isAdmin);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("my-attendance")]
        public async Task<IActionResult> GetMyAttendance()
        {
            var studentId = await _userClaimsHelper.GetStudentIdAsync();
            if (!studentId.HasValue)
                return Unauthorized("Student not found or user is not a student");

            var result = await _attendanceService.GetMyAttendanceAsync(studentId.Value);
            return StatusCode(result.StatusCode, result);
        }
    }
}

