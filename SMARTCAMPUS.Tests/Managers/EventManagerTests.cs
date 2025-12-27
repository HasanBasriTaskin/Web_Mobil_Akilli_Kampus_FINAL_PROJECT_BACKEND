using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Event;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class EventManagerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IQRCodeService> _mockQRCodeService;
        private readonly Mock<IWalletService> _mockWalletService;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly EventManager _manager;

        public EventManagerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockQRCodeService = new Mock<IQRCodeService>();
            _mockWalletService = new Mock<IWalletService>();
            
            var store = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

            _manager = new EventManager(
                _mockUnitOfWork.Object,
                _mockQRCodeService.Object,
                _mockWalletService.Object,
                _mockUserManager.Object);
        }

        [Fact]
        public async Task GetEventsAsync_ShouldReturnEvents()
        {
            // Arrange
            var category = new EventCategory { Id = 1, Name = "Conference", IsActive = true };
            var evt = new Event
            {
                Id = 1,
                Title = "Test Event",
                CategoryId = 1,
                Category = category,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(1).AddHours(2),
                Location = "Hall",
                Price = 0,
                Capacity = 100,
                RegisteredCount = 0,
                IsActive = true
            };

            _mockUnitOfWork.Setup(u => u.Events.GetEventsCountAsync(null, null, null, null, null))
                .ReturnsAsync(1);
            _mockUnitOfWork.Setup(u => u.Events.GetEventsFilteredAsync(null, null, null, null, null, 1, 20))
                .ReturnsAsync(new List<Event> { evt });

            var filter = new EventFilterDto();

            // Act
            var result = await _manager.GetEventsAsync(filter, 1, 20);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetEventByIdAsync_ShouldReturnEvent_WhenExists()
        {
            // Arrange
            var category = new EventCategory { Id = 1, Name = "Conference", IsActive = true };
            var user = new User { Id = "org1", UserName = "organizer" };
            var evt = new Event
            {
                Id = 1,
                Title = "Test Event",
                Description = "Test Description",
                CategoryId = 1,
                Category = category,
                CreatedByUserId = "org1",
                CreatedBy = user,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(1).AddHours(2),
                Location = "Hall",
                Price = 0,
                Capacity = 100,
                RegisteredCount = 0,
                IsActive = true,
                Registrations = new List<EventRegistration>()
            };

            _mockUnitOfWork.Setup(u => u.Events.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(evt);

            // Act
            var result = await _manager.GetEventByIdAsync(1, "user1");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Title.Should().Be("Test Event");
        }

        [Fact]
        public async Task GetEventByIdAsync_ShouldReturnFail_WhenNotFound()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.Events.GetByIdWithDetailsAsync(999))
                .ReturnsAsync((Event?)null);

            // Act
            var result = await _manager.GetEventByIdAsync(999);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task CreateEventAsync_ShouldReturnSuccess_WhenValid()
        {
            // Arrange
            var category = new EventCategory { Id = 1, Name = "Conference", IsActive = true };
            var user = new User { Id = "org1", UserName = "organizer" };

            _mockUnitOfWork.Setup(u => u.EventCategories.GetByIdAsync(1))
                .ReturnsAsync(category);
            _mockUnitOfWork.Setup(u => u.Events.AddAsync(It.IsAny<Event>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask);

            var createdEvent = new Event
            {
                Id = 1,
                Title = "New Event",
                Description = "Description",
                CategoryId = 1,
                Category = category,
                CreatedByUserId = "org1",
                CreatedBy = user,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(1).AddHours(2),
                Location = "Hall",
                Price = 0,
                Capacity = 100,
                RegisteredCount = 0,
                IsActive = true,
                Registrations = new List<EventRegistration>()
            };

            _mockUnitOfWork.Setup(u => u.Events.GetByIdWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(createdEvent);

            var dto = new EventCreateDto
            {
                Title = "New Event",
                Description = "Description",
                CategoryId = 1,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(1).AddHours(2),
                Location = "Hall",
                Price = 0,
                Capacity = 100
            };

            // Act
            var result = await _manager.CreateEventAsync("org1", dto);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            _mockUnitOfWork.Verify(u => u.Events.AddAsync(It.IsAny<Event>()), Times.Once);
        }

        [Fact]
        public async Task CreateEventAsync_ShouldReturnFail_WhenPastDate()
        {
            // Arrange
            var dto = new EventCreateDto
            {
                Title = "Past Event",
                Description = "Description",
                CategoryId = 1,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(-1).AddHours(2),
                Location = "Hall",
                Price = 0,
                Capacity = 100
            };

            // Act
            var result = await _manager.CreateEventAsync("org1", dto);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnSuccess_WhenValid()
        {
            // Arrange
            var category = new EventCategory { Id = 1, Name = "Conference", IsActive = true };
            var evt = new Event
            {
                Id = 1,
                Title = "Test Event",
                CategoryId = 1,
                Category = category,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(1).AddHours(2),
                Location = "Hall",
                Price = 0,
                Capacity = 100,
                RegisteredCount = 0,
                IsActive = true,
                Registrations = new List<EventRegistration>()
            };

            var user = new User { Id = "user1", UserName = "testuser" };

            _mockUnitOfWork.Setup(u => u.Events.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(evt);
            _mockUnitOfWork.Setup(u => u.EventRegistrations.GetByEventAndUserAsync(1, "user1"))
                .ReturnsAsync((EventRegistration?)null);
            _mockQRCodeService.Setup(q => q.GenerateQRCode("EVENT", It.IsAny<int>()))
                .Returns("QR-123");
            _mockUnitOfWork.Setup(u => u.EventRegistrations.AddAsync(It.IsAny<EventRegistration>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask);
            _mockUserManager.Setup(u => u.FindByIdAsync("user1"))
                .ReturnsAsync(user);

            // Act
            var result = await _manager.RegisterAsync("user1", 1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            _mockUnitOfWork.Verify(u => u.EventRegistrations.AddAsync(It.IsAny<EventRegistration>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnFail_WhenFull()
        {
            // Arrange
            var category = new EventCategory { Id = 1, Name = "Conference", IsActive = true };
            var evt = new Event
            {
                Id = 1,
                Title = "Test Event",
                CategoryId = 1,
                Category = category,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(1).AddHours(2),
                Location = "Hall",
                Price = 0,
                Capacity = 100,
                RegisteredCount = 100,
                IsActive = true
            };

            _mockUnitOfWork.Setup(u => u.Events.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(evt);

            // Act
            var result = await _manager.RegisterAsync("user1", 1);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task CancelRegistrationAsync_ShouldReturnSuccess()
        {
            // Arrange
            var category = new EventCategory { Id = 1, Name = "Conference", IsActive = true };
            var evt = new Event
            {
                Id = 1,
                Title = "Test Event",
                CategoryId = 1,
                Category = category,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(1).AddHours(2),
                RegisteredCount = 1,
                IsActive = true
            };

            var registration = new EventRegistration
            {
                Id = 1,
                EventId = 1,
                Event = evt,
                UserId = "user1",
                IsActive = true,
                RegistrationDate = DateTime.UtcNow.AddDays(-1)
            };

            _mockUnitOfWork.Setup(u => u.EventRegistrations.GetByEventAndUserAsync(1, "user1"))
                .ReturnsAsync(registration);
            _mockUnitOfWork.Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _manager.CancelRegistrationAsync("user1", 1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }
    }
}

