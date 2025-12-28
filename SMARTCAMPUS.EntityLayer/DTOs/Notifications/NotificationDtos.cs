using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.EntityLayer.DTOs.Notifications
{
    /// <summary>
    /// Bildirim görüntüleme DTO'su
    /// </summary>
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Bildirim oluşturma DTO'su
    /// </summary>
    public class CreateNotificationDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; } = NotificationType.Info;
        public NotificationCategory Category { get; set; } = NotificationCategory.System;
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
    }

    /// <summary>
    /// Toplu bildirim oluşturma DTO'su
    /// </summary>
    public class BroadcastNotificationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; } = NotificationType.Info;
        public NotificationCategory Category { get; set; } = NotificationCategory.System;
        public string? TargetRole { get; set; }
    }

    /// <summary>
    /// Bildirim tercihleri DTO'su
    /// </summary>
    public class NotificationPreferenceDto
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public bool InAppEnabled { get; set; }
        public bool EmailEnabled { get; set; }
    }

    /// <summary>
    /// Bildirim tercihleri güncelleme DTO'su
    /// </summary>
    public class UpdatePreferencesDto
    {
        public List<PreferenceItem> Preferences { get; set; } = new();
    }

    public class PreferenceItem
    {
        public NotificationCategory Category { get; set; }
        public bool InAppEnabled { get; set; }
        public bool EmailEnabled { get; set; }
    }

    /// <summary>
    /// Okunmamış bildirim sayısı DTO'su
    /// </summary>
    public class UnreadCountDto
    {
        public int Count { get; set; }
    }
}
