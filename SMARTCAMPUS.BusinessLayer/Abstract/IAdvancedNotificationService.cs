using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Notifications;
using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.BusinessLayer.Abstract
{
    /// <summary>
    /// Gelişmiş bildirim servisi (in-app + e-posta + SignalR)
    /// </summary>
    public interface IAdvancedNotificationService
    {
        /// <summary>
        /// Kullanıcıya bildirim gönderir
        /// </summary>
        Task<Response<NotificationDto>> SendNotificationAsync(CreateNotificationDto dto);

        /// <summary>
        /// Birden fazla kullanıcıya bildirim gönderir
        /// </summary>
        Task<Response<int>> SendBulkNotificationAsync(List<string> userIds, CreateNotificationDto dto);

        /// <summary>
        /// Tüm kullanıcılara veya belirli bir role bildirim yayınlar
        /// </summary>
        Task<Response<int>> BroadcastNotificationAsync(BroadcastNotificationDto dto);

        /// <summary>
        /// Kullanıcının bildirimlerini getirir
        /// </summary>
        Task<Response<List<NotificationDto>>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Okunmamış bildirim sayısını getirir
        /// </summary>
        Task<Response<UnreadCountDto>> GetUnreadCountAsync(string userId);

        /// <summary>
        /// Bildirimi okundu olarak işaretler
        /// </summary>
        Task<Response<NoDataDto>> MarkAsReadAsync(int notificationId, string userId);

        /// <summary>
        /// Tüm bildirimleri okundu olarak işaretler
        /// </summary>
        Task<Response<NoDataDto>> MarkAllAsReadAsync(string userId);

        /// <summary>
        /// Kullanıcının bildirim tercihlerini getirir
        /// </summary>
        Task<Response<List<NotificationPreferenceDto>>> GetPreferencesAsync(string userId);

        /// <summary>
        /// Bildirim tercihlerini günceller
        /// </summary>
        Task<Response<NoDataDto>> UpdatePreferencesAsync(string userId, UpdatePreferencesDto dto);

        /// <summary>
        /// Eski bildirimleri temizler
        /// </summary>
        Task<Response<int>> CleanupOldNotificationsAsync(int daysOld = 30);
    }
}
