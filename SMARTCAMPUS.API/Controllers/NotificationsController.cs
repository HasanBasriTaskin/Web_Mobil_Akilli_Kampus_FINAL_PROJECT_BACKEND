using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Notifications;
using System.Security.Claims;

namespace SMARTCAMPUS.API.Controllers
{
    /// <summary>
    /// Bildirim yönetimi endpoint'leri
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly IAdvancedNotificationService _notificationService;

        public NotificationsController(IAdvancedNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        /// <summary>
        /// Kullanıcının bildirimlerini getirir
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var response = await _notificationService.GetUserNotificationsAsync(GetUserId(), page, pageSize);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Okunmamış bildirim sayısını getirir
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var response = await _notificationService.GetUnreadCountAsync(GetUserId());
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Bildirimi okundu olarak işaretler
        /// </summary>
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var response = await _notificationService.MarkAsReadAsync(id, GetUserId());
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Tüm bildirimleri okundu olarak işaretler
        /// </summary>
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var response = await _notificationService.MarkAllAsReadAsync(GetUserId());
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Bildirim tercihlerini getirir
        /// </summary>
        [HttpGet("preferences")]
        public async Task<IActionResult> GetPreferences()
        {
            var response = await _notificationService.GetPreferencesAsync(GetUserId());
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Bildirim tercihlerini günceller
        /// </summary>
        [HttpPut("preferences")]
        public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesDto dto)
        {
            var response = await _notificationService.UpdatePreferencesAsync(GetUserId(), dto);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Tüm kullanıcılara veya belirli bir role bildirim gönderir (Admin only)
        /// </summary>
        [HttpPost("broadcast")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BroadcastNotification([FromBody] BroadcastNotificationDto dto)
        {
            var response = await _notificationService.BroadcastNotificationAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Belirli bir kullanıcıya bildirim gönderir (Admin only)
        /// </summary>
        [HttpPost("send")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendNotification([FromBody] CreateNotificationDto dto)
        {
            var response = await _notificationService.SendNotificationAsync(dto);
            return StatusCode(response.StatusCode, response);
        }
    }
}
