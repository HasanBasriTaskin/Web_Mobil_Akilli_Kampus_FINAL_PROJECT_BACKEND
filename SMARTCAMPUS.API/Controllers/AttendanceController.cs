using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

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

        [HttpPost("sessions")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> CreateSession([FromBody] AttendanceSessionDto sessionDto)
        {
            var result = await _attendanceService.CreateSessionAsync(sessionDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn([FromBody] AttendanceCheckInDto checkInDto)
        {
            // TODO: Get studentId from JWT token claims
            var studentId = 1; // Placeholder - should come from authenticated user
            var result = await _attendanceService.CheckInAsync(studentId, checkInDto);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("sessions/{sessionId}")]
        public async Task<IActionResult> GetSession(int sessionId)
        {
            var result = await _attendanceService.GetSessionByIdAsync(sessionId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("sessions/{sessionId}/records")]
        [Authorize(Roles = "Faculty,Admin")]
        public async Task<IActionResult> GetSessionRecords(int sessionId)
        {
            var result = await _attendanceService.GetSessionRecordsAsync(sessionId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("students/{studentId}")]
        public async Task<IActionResult> GetStudentAttendance(int studentId)
        {
            var result = await _attendanceService.GetStudentAttendanceAsync(studentId);
            return StatusCode(result.StatusCode, result);
        }
    }
}

