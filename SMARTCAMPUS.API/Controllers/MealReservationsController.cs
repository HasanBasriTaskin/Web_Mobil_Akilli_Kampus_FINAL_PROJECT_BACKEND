using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Reservation;
using SMARTCAMPUS.EntityLayer.Enums;
using System.Security.Claims;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class MealReservationsController : ControllerBase
    {
        private readonly IMealReservationService _reservationService;

        public MealReservationsController(IMealReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        #region User Operations

        /// <summary>
        /// Yemek rezervasyonu oluşturur
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MealReservationCreateDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User ID not found");

            var result = await _reservationService.CreateReservationAsync(userId, dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Kullanıcının rezervasyonlarını getirir
        /// </summary>
        [HttpGet("my-reservations")]
        public async Task<IActionResult> GetMyReservations(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User ID not found");

            var result = await _reservationService.GetMyReservationsAsync(userId, fromDate, toDate);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Rezervasyon detayını getirir
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User ID not found");

            var result = await _reservationService.GetReservationByIdAsync(userId, id);
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

        #region QR Operations

        /// <summary>
        /// QR kod ile rezervasyonu getirir
        /// </summary>
        [HttpGet("qr/{qrCode}")]
        [Authorize(Roles = "Admin,CafeteriaStaff")]
        public async Task<IActionResult> GetByQR(string qrCode)
        {
            var result = await _reservationService.GetReservationByQRAsync(qrCode);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// QR kod taraması yapar
        /// </summary>
        [HttpPost("scan")]
        [Authorize(Roles = "Admin,CafeteriaStaff")]
        public async Task<IActionResult> Scan([FromBody] MealScanDto dto)
        {
            var result = await _reservationService.ScanQRCodeAsync(dto.QRCode);
            return StatusCode(result.StatusCode, result);
        }

        #endregion

        #region Admin Operations

        /// <summary>
        /// Tarihe göre rezervasyonları getirir (Admin)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByDate(
            [FromQuery] DateTime date,
            [FromQuery] int? cafeteriaId,
            [FromQuery] MealType? mealType)
        {
            var result = await _reservationService.GetReservationsByDateAsync(date, cafeteriaId, mealType);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Süresi geçmiş rezervasyonları expire eder (Background Job)
        /// </summary>
        [HttpPost("expire")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExpireOld()
        {
            var result = await _reservationService.ExpireOldReservationsAsync();
            return StatusCode(result.StatusCode, result);
        }

        #endregion

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }

    public class MealScanDto
    {
        public string QRCode { get; set; } = null!;
    }
}
