using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.FacultyRequest;
using System.Security.Claims;

namespace SMARTCAMPUS.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class FacultyRequestsController : ControllerBase
    {
        private readonly IFacultyRequestService _facultyRequestService;

        public FacultyRequestsController(IFacultyRequestService facultyRequestService)
        {
            _facultyRequestService = facultyRequestService;
        }

        /// <summary>
        /// Akademisyenin bölümündeki uygun dersleri listeler
        /// </summary>
        [HttpGet("available-sections")]
        [Authorize(Roles = "Faculty")]
        public async Task<IActionResult> GetAvailableSections()
        {
            var facultyId = GetCurrentFacultyId();
            if (facultyId == null)
                return Unauthorized("Faculty ID not found");

            var result = await _facultyRequestService.GetAvailableSectionsAsync(facultyId.Value);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Akademisyen ders alma isteği gönderir
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Faculty")]
        public async Task<IActionResult> RequestSection([FromBody] CreateFacultyRequestDto dto)
        {
            var facultyId = GetCurrentFacultyId();
            if (facultyId == null)
                return Unauthorized("Faculty ID not found");

            var result = await _facultyRequestService.RequestSectionAsync(facultyId.Value, dto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Akademisyenin kendi isteklerini listeler
        /// </summary>
        [HttpGet("my")]
        [Authorize(Roles = "Faculty")]
        public async Task<IActionResult> GetMyRequests()
        {
            var facultyId = GetCurrentFacultyId();
            if (facultyId == null)
                return Unauthorized("Faculty ID not found");

            var result = await _facultyRequestService.GetMyRequestsAsync(facultyId.Value);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Admin: Tüm bekleyen istekleri listeler
        /// </summary>
        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPendingRequests()
        {
            var result = await _facultyRequestService.GetAllPendingRequestsAsync();
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Admin: İsteği onaylar
        /// </summary>
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveRequest(int id, [FromBody] ProcessFacultyRequestDto? dto)
        {
            var adminId = GetCurrentUserId();
            if (adminId == null)
                return Unauthorized("User ID not found");

            var result = await _facultyRequestService.ApproveRequestAsync(id, adminId, dto?.Note);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Admin: İsteği reddeder
        /// </summary>
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectRequest(int id, [FromBody] ProcessFacultyRequestDto? dto)
        {
            var adminId = GetCurrentUserId();
            if (adminId == null)
                return Unauthorized("User ID not found");

            var result = await _facultyRequestService.RejectRequestAsync(id, adminId, dto?.Note);
            return StatusCode(result.StatusCode, result);
        }

        private int? GetCurrentFacultyId()
        {
            var claim = User.FindFirst("FacultyId");
            if (claim != null && int.TryParse(claim.Value, out var id))
                return id;
            return null;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
