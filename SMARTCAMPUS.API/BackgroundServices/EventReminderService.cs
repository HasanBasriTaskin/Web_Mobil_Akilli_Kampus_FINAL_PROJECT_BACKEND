using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs.Notifications;
using SMARTCAMPUS.EntityLayer.Enums;

namespace SMARTCAMPUS.API.BackgroundServices
{
    /// <summary>
    /// Yaklaşan etkinlikler için katılımcılara hatırlatma bildirimi gönderen background service
    /// (24 saat ve 1 saat önce)
    /// </summary>
    public class EventReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventReminderService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(30); // 30 dakikada bir kontrol

        public EventReminderService(
            IServiceProvider serviceProvider,
            ILogger<EventReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EventReminderService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndSendRemindersAsync();
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in EventReminderService");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("EventReminderService stopped.");
        }

        private async Task CheckAndSendRemindersAsync()
        {
            _logger.LogDebug("Checking for upcoming events...");

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<CampusContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<IAdvancedNotificationService>();

            var now = DateTime.UtcNow;
            var reminderCount = 0;

            // 24 saat sonra başlayacak etkinlikler (23.5 - 24.5 saat arası)
            var events24h = await context.Events
                .Where(e => e.IsActive &&
                            e.StartDate > now.AddHours(23.5) &&
                            e.StartDate <= now.AddHours(24.5))
                .ToListAsync();

            // 1 saat sonra başlayacak etkinlikler (0.5 - 1.5 saat arası)
            var events1h = await context.Events
                .Where(e => e.IsActive &&
                            e.StartDate > now.AddMinutes(30) &&
                            e.StartDate <= now.AddMinutes(90))
                .ToListAsync();

            // 24 saat hatırlatmaları
            foreach (var evt in events24h)
            {
                var registrations = await context.EventRegistrations
                    .Where(r => r.EventId == evt.Id && r.IsActive)
                    .ToListAsync();

                foreach (var registration in registrations)
                {
                    try
                    {
                        await notificationService.SendNotificationAsync(new CreateNotificationDto
                        {
                            UserId = registration.UserId,
                            Title = "Etkinlik Hatırlatması - 24 Saat",
                            Message = $"\"{evt.Title}\" etkinliği yarın {evt.StartDate.ToLocalTime():HH:mm}'de başlayacak. " +
                                      $"Konum: {evt.Location ?? "Belirtilmemiş"}",
                            Type = NotificationType.Reminder,
                            Category = NotificationCategory.Event,
                            RelatedEntityType = "Event",
                            RelatedEntityId = evt.Id
                        });
                        reminderCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending 24h reminder for event {EventId} to user {UserId}",
                            evt.Id, registration.UserId);
                    }
                }
            }

            // 1 saat hatırlatmaları
            foreach (var evt in events1h)
            {
                var registrations = await context.EventRegistrations
                    .Where(r => r.EventId == evt.Id && r.IsActive)
                    .ToListAsync();

                foreach (var registration in registrations)
                {
                    try
                    {
                        await notificationService.SendNotificationAsync(new CreateNotificationDto
                        {
                            UserId = registration.UserId,
                            Title = "Etkinlik Başlamak Üzere - 1 Saat",
                            Message = $"\"{evt.Title}\" etkinliği 1 saat içinde başlayacak! " +
                                      $"Konum: {evt.Location ?? "Belirtilmemiş"}",
                            Type = NotificationType.Reminder,
                            Category = NotificationCategory.Event,
                            RelatedEntityType = "Event",
                            RelatedEntityId = evt.Id
                        });
                        reminderCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending 1h reminder for event {EventId} to user {UserId}",
                            evt.Id, registration.UserId);
                    }
                }
            }

            if (reminderCount > 0)
            {
                _logger.LogInformation("Event reminders sent: {Count}", reminderCount);
            }
        }
    }
}
