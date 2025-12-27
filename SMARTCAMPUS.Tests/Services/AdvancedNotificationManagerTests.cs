using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using SMARTCAMPUS.API.Hubs;
using SMARTCAMPUS.API.Services;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Notifications;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;
using System.Security.Claims;
using Xunit;

namespace SMARTCAMPUS.Tests.Services
{
    public class AdvancedNotificationManagerTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly Mock<IHubContext<NotificationHub>> _mockHubContext;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IClientProxy> _mockClientProxy;
        private readonly AdvancedNotificationManager _manager;

        public AdvancedNotificationManagerTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new CampusContext(options);

            _mockHubContext = new Mock<IHubContext<NotificationHub>>();
            _mockClientProxy = new Mock<IClientProxy>();
            var mockClients = new Mock<IHubClients>();
            var mockGroupManager = new Mock<IGroupManager>();
            mockClients.Setup(x => x.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
            mockClients.Setup(x => x.All).Returns(_mockClientProxy.Object);
            _mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);

            var store = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

            _manager = new AdvancedNotificationManager(_context, _mockHubContext.Object, _mockUserManager.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task SendBulkNotificationAsync_ShouldSendToMultipleUsers()
        {
            var dto = new CreateNotificationDto
            {
                Title = "Test",
                Message = "Message",
                Type = NotificationType.Info,
                Category = NotificationCategory.System
            };
            var userIds = new List<string> { "user1", "user2" };

            var result = await _manager.SendBulkNotificationAsync(userIds, dto);

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().Be(2);
            _context.Notifications.Count().Should().Be(2);
        }

        [Fact]
        public async Task BroadcastNotificationAsync_ShouldBroadcastToAllUsers_WhenNoRole()
        {
            var user = new User { Id = "user1", UserName = "user1", FullName = "User 1", IsActive = true };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var dto = new BroadcastNotificationDto
            {
                Title = "Broadcast",
                Message = "Message",
                Type = NotificationType.Info,
                Category = NotificationCategory.System
            };

            var result = await _manager.BroadcastNotificationAsync(dto);

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().Be(1);
        }

        [Fact]
        public async Task BroadcastNotificationAsync_ShouldBroadcastToRole_WhenRoleSpecified()
        {
            var user = new User { Id = "user1", UserName = "user1", FullName = "User 1", IsActive = true };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _mockUserManager.Setup(x => x.GetUsersInRoleAsync("Admin"))
                .ReturnsAsync(new List<User> { user });

            var dto = new BroadcastNotificationDto
            {
                Title = "Broadcast",
                Message = "Message",
                Type = NotificationType.Info,
                Category = NotificationCategory.System,
                TargetRole = "Admin"
            };

            var result = await _manager.BroadcastNotificationAsync(dto);

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().Be(1);
        }

        [Fact]
        public async Task GetUserNotificationsAsync_ShouldReturnPagedNotifications()
        {
            var notification1 = new Notification
            {
                UserId = "user1",
                Title = "Test1",
                Message = "Message1",
                Type = NotificationType.Info,
                Category = NotificationCategory.System,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-1)
            };
            var notification2 = new Notification
            {
                UserId = "user1",
                Title = "Test2",
                Message = "Message2",
                Type = NotificationType.Info,
                Category = NotificationCategory.System,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            _context.Notifications.AddRange(notification1, notification2);
            await _context.SaveChangesAsync();

            var result = await _manager.GetUserNotificationsAsync("user1", 1, 1);

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data[0].Title.Should().Be("Test2");
        }

        [Fact]
        public async Task MarkAsReadAsync_ShouldMarkNotificationAsRead()
        {
            var notification = new Notification
            {
                UserId = "user1",
                Title = "Test",
                Message = "Message",
                Type = NotificationType.Info,
                Category = NotificationCategory.System,
                IsRead = false,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var result = await _manager.MarkAsReadAsync(notification.Id, "user1");

            result.IsSuccessful.Should().BeTrue();
            notification.IsRead.Should().BeTrue();
            notification.ReadAt.Should().NotBeNull();
        }

        [Fact]
        public async Task MarkAsReadAsync_ShouldReturnFail_WhenNotFound()
        {
            var result = await _manager.MarkAsReadAsync(999, "user1");

            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task MarkAllAsReadAsync_ShouldMarkAllAsRead()
        {
            var notification1 = new Notification
            {
                UserId = "user1",
                Title = "Test1",
                Message = "Message1",
                Type = NotificationType.Info,
                Category = NotificationCategory.System,
                IsRead = false,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            var notification2 = new Notification
            {
                UserId = "user1",
                Title = "Test2",
                Message = "Message2",
                Type = NotificationType.Info,
                Category = NotificationCategory.System,
                IsRead = false,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            _context.Notifications.AddRange(notification1, notification2);
            await _context.SaveChangesAsync();

            var result = await _manager.MarkAllAsReadAsync("user1");

            result.IsSuccessful.Should().BeTrue();
            notification1.IsRead.Should().BeTrue();
            notification2.IsRead.Should().BeTrue();
        }

        [Fact]
        public async Task GetPreferencesAsync_ShouldCreateDefaults_WhenNoneExist()
        {
            var result = await _manager.GetPreferencesAsync("user1");

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().NotBeEmpty();
            _context.NotificationPreferences.Count().Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task UpdatePreferencesAsync_ShouldUpdateExistingPreferences()
        {
            var preference = new NotificationPreference
            {
                UserId = "user1",
                Category = NotificationCategory.System,
                InAppEnabled = true,
                EmailEnabled = false,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            _context.NotificationPreferences.Add(preference);
            await _context.SaveChangesAsync();

            var dto = new UpdatePreferencesDto
            {
                Preferences = new List<PreferenceItem>
                {
                    new PreferenceItem
                    {
                        Category = NotificationCategory.System,
                        InAppEnabled = false,
                        EmailEnabled = true
                    }
                }
            };

            var result = await _manager.UpdatePreferencesAsync("user1", dto);

            result.IsSuccessful.Should().BeTrue();
            preference.InAppEnabled.Should().BeFalse();
            preference.EmailEnabled.Should().BeTrue();
        }

        [Fact]
        public async Task UpdatePreferencesAsync_ShouldCreateNewPreferences_WhenNotExist()
        {
            var dto = new UpdatePreferencesDto
            {
                Preferences = new List<PreferenceItem>
                {
                    new PreferenceItem
                    {
                        Category = NotificationCategory.System,
                        InAppEnabled = true,
                        EmailEnabled = false
                    }
                }
            };

            var result = await _manager.UpdatePreferencesAsync("user1", dto);

            result.IsSuccessful.Should().BeTrue();
            _context.NotificationPreferences.Count().Should().Be(1);
        }

        [Fact]
        public async Task CleanupOldNotificationsAsync_ShouldRemoveOldReadNotifications()
        {
            var oldNotification = new Notification
            {
                UserId = "user1",
                Title = "Old",
                Message = "Message",
                Type = NotificationType.Info,
                Category = NotificationCategory.System,
                IsRead = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-31)
            };
            var newNotification = new Notification
            {
                UserId = "user1",
                Title = "New",
                Message = "Message",
                Type = NotificationType.Info,
                Category = NotificationCategory.System,
                IsRead = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-10)
            };
            _context.Notifications.AddRange(oldNotification, newNotification);
            await _context.SaveChangesAsync();

            var result = await _manager.CleanupOldNotificationsAsync(30);

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().Be(1);
            _context.Notifications.Count().Should().Be(1);
        }
    }
}

