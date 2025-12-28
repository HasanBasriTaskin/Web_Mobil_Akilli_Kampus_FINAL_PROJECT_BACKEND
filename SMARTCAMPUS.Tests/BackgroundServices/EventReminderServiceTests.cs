using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SMARTCAMPUS.API.BackgroundServices;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.Models;
using System.Reflection;
using Xunit;

namespace SMARTCAMPUS.Tests.BackgroundServices
{
    public class EventReminderServiceTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly Mock<IAdvancedNotificationService> _mockNotificationService;
        private readonly Mock<ILogger<EventReminderService>> _mockLogger;
        private readonly IServiceProvider _serviceProvider;
        private readonly EventReminderService _service;

        public EventReminderServiceTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new CampusContext(options);
            _mockNotificationService = new Mock<IAdvancedNotificationService>();
            _mockLogger = new Mock<ILogger<EventReminderService>>();

            var services = new ServiceCollection();
            services.AddSingleton(_context);
            services.AddSingleton(_mockNotificationService.Object);
            _serviceProvider = services.BuildServiceProvider();

            _service = new EventReminderService(_serviceProvider, _mockLogger.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void Constructor_ShouldInitialize()
        {
            _service.Should().NotBeNull();
        }

        [Fact]
        public async Task CheckAndSendRemindersAsync_ShouldSend24HourReminder_WhenEventStartsIn24Hours()
        {
            // Arrange
            var user = new User { Id = "u1", FullName = "User", IsActive = true };
            var category = new EventCategory { Id = 1, Name = "Conference", IsActive = true };
            var evt = new Event
            {
                Id = 1,
                Title = "Test Event",
                Description = "Test",
                StartDate = DateTime.UtcNow.AddHours(24),
                EndDate = DateTime.UtcNow.AddHours(26),
                Location = "Hall A",
                CategoryId = 1,
                Category = category,
                CreatedByUserId = "u1",
                CreatedBy = user,
                IsActive = true
            };
            var registration = new EventRegistration
            {
                Id = 1,
                EventId = 1,
                Event = evt,
                UserId = "u1",
                User = user,
                QRCode = "QR-123",
                RegistrationDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            _context.EventCategories.Add(category);
            _context.Events.Add(evt);
            _context.EventRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            _mockNotificationService.Setup(x => x.SendNotificationAsync(It.IsAny<SMARTCAMPUS.EntityLayer.DTOs.Notifications.CreateNotificationDto>()))
                .ReturnsAsync(new SMARTCAMPUS.BusinessLayer.Common.Response<SMARTCAMPUS.EntityLayer.DTOs.Notifications.NotificationDto> { IsSuccessful = true });

            // Act
            var method = typeof(EventReminderService).GetMethod("CheckAndSendRemindersAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)method!.Invoke(_service, null)!;

            // Assert
            _mockNotificationService.Verify(x => x.SendNotificationAsync(It.Is<SMARTCAMPUS.EntityLayer.DTOs.Notifications.CreateNotificationDto>(
                dto => dto.Title == "Etkinlik Hatırlatması - 24 Saat" && dto.UserId == "u1")), Times.Once);
        }

        [Fact]
        public async Task CheckAndSendRemindersAsync_ShouldSend1HourReminder_WhenEventStartsIn1Hour()
        {
            // Arrange
            var user = new User { Id = "u1", FullName = "User", IsActive = true };
            var category = new EventCategory { Id = 1, Name = "Conference", IsActive = true };
            var evt = new Event
            {
                Id = 1,
                Title = "Test Event",
                Description = "Test",
                StartDate = DateTime.UtcNow.AddMinutes(60),
                EndDate = DateTime.UtcNow.AddHours(2),
                Location = "Hall A",
                CategoryId = 1,
                Category = category,
                CreatedByUserId = "u1",
                CreatedBy = user,
                IsActive = true
            };
            var registration = new EventRegistration
            {
                Id = 1,
                EventId = 1,
                Event = evt,
                UserId = "u1",
                User = user,
                QRCode = "QR-123",
                RegistrationDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            _context.EventCategories.Add(category);
            _context.Events.Add(evt);
            _context.EventRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            _mockNotificationService.Setup(x => x.SendNotificationAsync(It.IsAny<SMARTCAMPUS.EntityLayer.DTOs.Notifications.CreateNotificationDto>()))
                .ReturnsAsync(new SMARTCAMPUS.BusinessLayer.Common.Response<SMARTCAMPUS.EntityLayer.DTOs.Notifications.NotificationDto> { IsSuccessful = true });

            // Act
            var method = typeof(EventReminderService).GetMethod("CheckAndSendRemindersAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)method!.Invoke(_service, null)!;

            // Assert
            _mockNotificationService.Verify(x => x.SendNotificationAsync(It.Is<SMARTCAMPUS.EntityLayer.DTOs.Notifications.CreateNotificationDto>(
                dto => dto.Title == "Etkinlik Başlamak Üzere - 1 Saat" && dto.UserId == "u1")), Times.Once);
        }

        [Fact]
        public async Task CheckAndSendRemindersAsync_ShouldNotSendReminder_WhenEventNotInTimeWindow()
        {
            // Arrange
            var user = new User { Id = "u1", FullName = "User", IsActive = true };
            var category = new EventCategory { Id = 1, Name = "Conference", IsActive = true };
            var evt = new Event
            {
                Id = 1,
                Title = "Test Event",
                Description = "Test",
                StartDate = DateTime.UtcNow.AddHours(48), // 48 saat sonra - zaman penceresi dışında
                EndDate = DateTime.UtcNow.AddHours(50),
                Location = "Hall A",
                CategoryId = 1,
                Category = category,
                CreatedByUserId = "u1",
                CreatedBy = user,
                IsActive = true
            };
            var registration = new EventRegistration
            {
                Id = 1,
                EventId = 1,
                Event = evt,
                UserId = "u1",
                User = user,
                QRCode = "QR-123",
                RegistrationDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            _context.EventCategories.Add(category);
            _context.Events.Add(evt);
            _context.EventRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            // Act
            var method = typeof(EventReminderService).GetMethod("CheckAndSendRemindersAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            await (Task)method!.Invoke(_service, null)!;

            // Assert
            _mockNotificationService.Verify(x => x.SendNotificationAsync(It.IsAny<SMARTCAMPUS.EntityLayer.DTOs.Notifications.CreateNotificationDto>()), Times.Never);
        }
    }
}

