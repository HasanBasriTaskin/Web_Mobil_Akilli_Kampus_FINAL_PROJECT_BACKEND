using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Scheduling;
using System.Security.Claims;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class ClassroomReservationsController : ControllerBase
    {
        private readonly IClassroomReservationService _reservationService;

        public ClassroomReservationsController(IClassroomReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        #region User Operations

        /// <summary>
        /// Kullanıcının rezervasyonlarını getirir
        /// </summary>
        [HttpGet("my-reservations")]
        public async Task<IActionResult> GetMyReservations()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User ID not found");

            var result = await _reservationService.GetMyReservationsAsync(userId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Sınıf rezervasyonu oluşturur
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Faculty,Student")]
        public async Task<IActionResult> Create([FromBody] ClassroomReservationCreateDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User ID not found");

            var result = await _reservationService.CreateReservationAsync(userId, dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Rezervasyonu iptal eder
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User ID not found");

            var result = await _reservationService.CancelReservationAsync(userId, id);
            return StatusCode(result.StatusCode, result);
        }

        #endregion

        #region Public Operations

        /// <summary>
        /// Sınıf uygunluğunu getirir
        /// </summary>
        [HttpGet("availability/{classroomId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailability(int classroomId, [FromQuery] DateTime date)
        {
            var result = await _reservationService.GetClassroomAvailabilityAsync(classroomId, date);
            return StatusCode(result.StatusCode, result);
        }

        #endregion

        #region Admin Operations

        /// <summary>
        /// Bekleyen rezervasyonları getirir
        /// </summary>
        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPending()
        {
            var result = await _reservationService.GetPendingReservationsAsync();
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Tarihe göre rezervasyonları getirir
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByDate([FromQuery] DateTime date, [FromQuery] int? classroomId)
        {
            var result = await _reservationService.GetReservationsByDateAsync(date, classroomId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Rezervasyonu onaylar
        /// </summary>
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id, [FromBody] ReservationApprovalDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User ID not found");

            var result = await _reservationService.ApproveReservationAsync(userId, id, dto.Notes);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Rezervasyonu reddeder
        /// </summary>
        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id, [FromBody] ReservationRejectionDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User ID not found");

            var result = await _reservationService.RejectReservationAsync(userId, id, dto.Reason);
            return StatusCode(result.StatusCode, result);
        }

        #endregion

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }

    public class ReservationApprovalDto
    {
        public string? Notes { get; set; }
    }

    public class ReservationRejectionDto
    {
        public string Reason { get; set; } = null!;
    }
}
