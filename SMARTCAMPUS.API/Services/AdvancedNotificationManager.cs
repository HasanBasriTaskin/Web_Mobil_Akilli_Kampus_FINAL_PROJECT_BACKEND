using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.API.Hubs;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Notifications;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.API.Services
{
    /// <summary>
    /// Gelişmiş bildirim servisi implementasyonu (SignalR destekli)
    /// </summary>
    public class AdvancedNotificationManager : IAdvancedNotificationService
    {
        private readonly CampusContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly UserManager<User> _userManager;

        public AdvancedNotificationManager(
            CampusContext context,
            IHubContext<NotificationHub> hubContext,
            UserManager<User> userManager)
        {
            _context = context;
            _hubContext = hubContext;
            _userManager = userManager;
        }

        public async Task<Response<NotificationDto>> SendNotificationAsync(CreateNotificationDto dto)
        {
            var notification = new Notification
            {
                UserId = dto.UserId,
                Title = dto.Title,
                Message = dto.Message,
                Type = dto.Type,
                Category = dto.Category,
                RelatedEntityType = dto.RelatedEntityType,
                RelatedEntityId = dto.RelatedEntityId,
                IsRead = false,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var notificationDto = MapToDto(notification);

            // SignalR ile anlık bildirim gönder
            await _hubContext.Clients.Group($"user_{dto.UserId}")
                .SendAsync("ReceiveNotification", notificationDto);

            return Response<NotificationDto>.Success(notificationDto, 201);
        }

        public async Task<Response<int>> SendBulkNotificationAsync(List<string> userIds, CreateNotificationDto dto)
        {
            var notifications = userIds.Select(userId => new Notification
            {
                UserId = userId,
                Title = dto.Title,
                Message = dto.Message,
                Type = dto.Type,
                Category = dto.Category,
                RelatedEntityType = dto.RelatedEntityType,
                RelatedEntityId = dto.RelatedEntityId,
                IsRead = false,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            }).ToList();

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            // SignalR ile anlık bildirim gönder
            foreach (var notification in notifications)
            {
                var notificationDto = MapToDto(notification);
                await _hubContext.Clients.Group($"user_{notification.UserId}")
                    .SendAsync("ReceiveNotification", notificationDto);
            }

            return Response<int>.Success(notifications.Count, 200);
        }

        public async Task<Response<int>> BroadcastNotificationAsync(BroadcastNotificationDto dto)
        {
            List<User> targetUsers;

            if (!string.IsNullOrEmpty(dto.TargetRole))
            {
                targetUsers = (await _userManager.GetUsersInRoleAsync(dto.TargetRole)).ToList();
            }
            else
            {
                targetUsers = await _context.Users.Where(u => u.IsActive).ToListAsync();
            }

            var notifications = targetUsers.Select(user => new Notification
            {
                UserId = user.Id,
                Title = dto.Title,
                Message = dto.Message,
                Type = dto.Type,
                Category = dto.Category,
                IsRead = false,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            }).ToList();

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            // SignalR ile toplu bildirim
            var notificationDto = new NotificationDto
            {
                Title = dto.Title,
                Message = dto.Message,
                Type = dto.Type.ToString(),
                Category = dto.Category.ToString(),
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            };

            if (!string.IsNullOrEmpty(dto.TargetRole))
            {
                await _hubContext.Clients.Group($"role_{dto.TargetRole}")
                    .SendAsync("ReceiveNotification", notificationDto);
            }
            else
            {
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", notificationDto);
            }

            return Response<int>.Success(notifications.Count, 200);
        }

        public async Task<Response<List<NotificationDto>>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.IsActive)
                .OrderByDescending(n => n.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = notifications.Select(MapToDto).ToList();
            return Response<List<NotificationDto>>.Success(dtos, 200);
        }

        public async Task<Response<UnreadCountDto>> GetUnreadCountAsync(string userId)
        {
            var count = await _context.Notifications
                .CountAsync(n => n.UserId == userId && n.IsActive && !n.IsRead);

            return Response<UnreadCountDto>.Success(new UnreadCountDto { Count = count }, 200);
        }

        public async Task<Response<NoDataDto>> MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && n.IsActive);

            if (notification == null)
            {
                return Response<NoDataDto>.Fail("Bildirim bulunamadı", 404);
            }

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> MarkAllAsReadAsync(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.IsActive && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<List<NotificationPreferenceDto>>> GetPreferencesAsync(string userId)
        {
            var preferences = await _context.NotificationPreferences
                .Where(p => p.UserId == userId && p.IsActive)
                .ToListAsync();

            // Eğer tercihler yoksa, varsayılan tercihleri oluştur
            if (!preferences.Any())
            {
                preferences = await CreateDefaultPreferencesAsync(userId);
            }

            var dtos = preferences.Select(p => new NotificationPreferenceDto
            {
                Id = p.Id,
                Category = p.Category.ToString(),
                InAppEnabled = p.InAppEnabled,
                EmailEnabled = p.EmailEnabled
            }).ToList();

            return Response<List<NotificationPreferenceDto>>.Success(dtos, 200);
        }

        public async Task<Response<NoDataDto>> UpdatePreferencesAsync(string userId, UpdatePreferencesDto dto)
        {
            foreach (var item in dto.Preferences)
            {
                var preference = await _context.NotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.Category == item.Category);

                if (preference == null)
                {
                    preference = new NotificationPreference
                    {
                        UserId = userId,
                        Category = item.Category,
                        InAppEnabled = item.InAppEnabled,
                        EmailEnabled = item.EmailEnabled,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.NotificationPreferences.Add(preference);
                }
                else
                {
                    preference.InAppEnabled = item.InAppEnabled;
                    preference.EmailEnabled = item.EmailEnabled;
                    preference.UpdatedDate = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<int>> CleanupOldNotificationsAsync(int daysOld = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var oldNotifications = await _context.Notifications
                .Where(n => n.CreatedDate < cutoffDate && n.IsRead)
                .ToListAsync();

            _context.Notifications.RemoveRange(oldNotifications);
            await _context.SaveChangesAsync();

            return Response<int>.Success(oldNotifications.Count, 200);
        }

        #region Private Methods

        private static NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type.ToString(),
                Category = notification.Category.ToString(),
                IsRead = notification.IsRead,
                ReadAt = notification.ReadAt,
                RelatedEntityType = notification.RelatedEntityType,
                RelatedEntityId = notification.RelatedEntityId,
                CreatedDate = notification.CreatedDate
            };
        }

        private async Task<List<NotificationPreference>> CreateDefaultPreferencesAsync(string userId)
        {
            var categories = Enum.GetValues<NotificationCategory>();
            var preferences = categories.Select(category => new NotificationPreference
            {
                UserId = userId,
                Category = category,
                InAppEnabled = true,
                EmailEnabled = category == NotificationCategory.Academic || category == NotificationCategory.Attendance,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            }).ToList();

            _context.NotificationPreferences.AddRange(preferences);
            await _context.SaveChangesAsync();
            return preferences;
        }

        #endregion
    }
}
