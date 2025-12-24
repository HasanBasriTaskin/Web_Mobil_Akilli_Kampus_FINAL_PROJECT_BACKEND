namespace SMARTCAMPUS.EntityLayer.Enums
{
    /// <summary>
    /// Bildirim türleri
    /// </summary>
    public enum NotificationType
    {
        Info = 0,
        Success = 1,
        Warning = 2,
        Error = 3,
        Reminder = 4
    }

    /// <summary>
    /// Bildirim kategorileri
    /// </summary>
    public enum NotificationCategory
    {
        System = 0,
        Academic = 1,
        Attendance = 2,
        Event = 3,
        Meal = 4,
        Payment = 5,
        Announcement = 6
    }

    /// <summary>
    /// Sensör türleri (IoT)
    /// </summary>
    public enum SensorType
    {
        Temperature = 0,
        Humidity = 1,
        Occupancy = 2,
        Energy = 3,
        AirQuality = 4
    }

    /// <summary>
    /// Risk seviyeleri
    /// </summary>
    public enum RiskLevel
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }
}
