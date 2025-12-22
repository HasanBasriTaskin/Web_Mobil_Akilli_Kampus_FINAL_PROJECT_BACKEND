using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Event;
using System.Security.Claims;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        #region Public Operations

        /// <summary>
        /// Etkinlik listesini getirir (sayfalı, filtreli)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetEvents(
            [FromQuery] EventFilterDto filter,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _eventService.GetEventsAsync(filter, page, pageSize);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Etkinlik detayını getirir
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetCurrentUserId();
            var result = await _eventService.GetEventByIdAsync(id, userId);
            return StatusCode(result.StatusCode, result);
        }

        #endregion

        #region Organizer Operations

        /// <summary>
        /// Yeni etkinlik oluşturur
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Faculty,Admin,EventOrganizer")]
        public async Task<IActionResult> Create([FromBody] EventCreateDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User ID not found");

            var result = await _eventService.CreateEventAsync(userId, dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Etkinliği günceller
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Faculty,Admin,EventOrganizer")]
        public async Task<IActionResult> Update(int id, [FromBody] EventUpdateDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User ID not found");

            var result = await _eventService.UpdateEventAsync(userId, id, dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Etkinliği siler (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Faculty,Admin,EventOrganizer")]
        public async Task<IActionResult> Delete(int id, [FromQuery] bool force = false)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User ID not found");

            var result = await _eventService.DeleteEventAsync(userId, id, force);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Etkinliği yayınlar
        /// </summary>
        [HttpPut("{id}/publish")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Publish(int id)
        {
            var result = await _eventService.PublishEventAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Etkinliği iptal eder (iade dahil)
        /// </summary>
        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cancel(int id, [FromBody] EventCancelDto dto)
        {
            var result = await _eventService.CancelEventAsync(id, dto.Reason);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Etkinlik kayıtlarını getirir
        /// </summary>
        [HttpGet("{id}/registrations")]
        [Authorize(Roles = "Faculty,Admin,EventOrganizer")]
        public async Task<IActionResult> GetRegistrations(int id)
        {
            var result = await _eventService.GetEventRegistrationsAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        #endregion

        #region Registration Operations

        /// <summary>
        /// Etkinliğe kayıt olur
        /// </summary>
        [HttpPost("{id}/register")]
        [Authorize]
        public async Task<IActionResult> Register(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User ID not found");

            var result = await _eventService.RegisterAsync(userId, id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Etkinlik kaydını iptal eder
        /// </summary>
        [HttpDelete("{id}/register")]
        [Authorize]
        public async Task<IActionResult> CancelRegistration(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User ID not found");

            var result = await _eventService.CancelRegistrationAsync(userId, id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Kullanıcının kayıtlı olduğu etkinlikleri getirir
        /// </summary>
        [HttpGet("my-registrations")]
        [Authorize]
        public async Task<IActionResult> GetMyRegistrations()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User ID not found");

            var result = await _eventService.GetMyRegistrationsAsync(userId);
            return StatusCode(result.StatusCode, result);
        }

        #endregion

        #region Waitlist Operations

        /// <summary>
        /// Bekleme listesine katılır
        /// </summary>
        [HttpPost("{id}/waitlist")]
        [Authorize]
        public async Task<IActionResult> JoinWaitlist(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User ID not found");

            var result = await _eventService.JoinWaitlistAsync(userId, id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Bekleme listesinden çıkar
        /// </summary>
        [HttpDelete("{id}/waitlist")]
        [Authorize]
        public async Task<IActionResult> LeaveWaitlist(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User ID not found");

            var result = await _eventService.LeaveWaitlistAsync(userId, id);
            return StatusCode(result.StatusCode, result);
        }

        #endregion

        #region Check-in Operations

        /// <summary>
        /// QR kod ile check-in yapar
        /// </summary>
        [HttpPost("check-in")]
        [Authorize(Roles = "Faculty,Admin,EventOrganizer")]
        public async Task<IActionResult> CheckIn([FromBody] EventCheckInDto dto)
        {
            var result = await _eventService.CheckInAsync(dto.QRCode);
            return StatusCode(result.StatusCode, result);
        }

        #endregion

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }

    public class EventCancelDto
    {
        public string Reason { get; set; } = null!;
    }

    public class EventCheckInDto
    {
        public string QRCode { get; set; } = null!;
    }
}
