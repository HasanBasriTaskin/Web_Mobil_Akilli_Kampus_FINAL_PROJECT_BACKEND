using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Event;
using SMARTCAMPUS.EntityLayer.DTOs.Wallet;
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
            var organizer = new User { Id = "org1", UserName = "organizer" };
            var evt = new Event
            {
                Id = 1,
                Title = "Test Event",
                Description = "Test Description",
                CategoryId = 1,
                Category = category,
                CreatedByUserId = "org1",
                CreatedBy = organizer,
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
                RegisteredCount = 100,
                IsActive = true
            };

            evt.Registrations = new List<EventRegistration>();
            _mockUnitOfWork.Setup(u => u.Events.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(evt);
            _mockUnitOfWork.Setup(u => u.EventRegistrations.IsUserRegisteredAsync(1, "user1"))
                .ReturnsAsync(false);

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
            _mockUnitOfWork.Setup(u => u.Events.GetByIdAsync(1))
                .ReturnsAsync(evt);
            _mockUnitOfWork.Setup(u => u.EventWaitlists.GetNextInQueueAsync(1))
                .ReturnsAsync((EventWaitlist?)null);
            _mockUnitOfWork.Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _manager.CancelRegistrationAsync("user1", 1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateEventAsync_ShouldReturnSuccess()
        {
            var category = new EventCategory { Id = 1, Name = "Conference", IsActive = true };
            var user = new User { Id = "org1", UserName = "organizer" };
            var evt = new Event
            {
                Id = 1,
                Title = "Old Title",
                Description = "Old Description",
                CategoryId = 1,
                Category = category,
                CreatedByUserId = "org1",
                CreatedBy = user,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(1).AddHours(2),
                Location = "Hall",
                IsActive = true,
                Registrations = new List<EventRegistration>()
            };

            _mockUnitOfWork.Setup(u => u.Events.GetByIdAsync(1))
                .ReturnsAsync(evt);
            _mockUnitOfWork.Setup(u => u.Events.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(evt);
            _mockUnitOfWork.Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask);

            var dto = new EventUpdateDto { Title = "New Title" };

            var result = await _manager.UpdateEventAsync("org1", 1, dto);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteEventAsync_ShouldReturnSuccess()
        {
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
                IsActive = true,
                Registrations = new List<EventRegistration>()
            };

            _mockUnitOfWork.Setup(u => u.Events.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(evt);
            _mockUnitOfWork.Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask);

            var result = await _manager.DeleteEventAsync("org1", 1);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task PublishEventAsync_ShouldReturnSuccess()
        {
            var evt = new Event { Id = 1, Title = "Event", IsActive = false };

            _mockUnitOfWork.Setup(u => u.Events.GetByIdAsync(1))
                .ReturnsAsync(evt);
            _mockUnitOfWork.Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask);

            var result = await _manager.PublishEventAsync(1);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task GetMyRegistrationsAsync_ShouldReturnRegistrations()
        {
            var registrations = new List<EventRegistration>
            {
                new EventRegistration { Id = 1, UserId = "user1", EventId = 1, IsActive = true }
            };

            _mockUnitOfWork.Setup(u => u.EventRegistrations.GetByUserIdAsync("user1"))
                .ReturnsAsync(registrations);

            var result = await _manager.GetMyRegistrationsAsync("user1");

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task CancelEventAsync_ShouldReturnSuccess_WhenEventExists()
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
            _mockUnitOfWork.Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _manager.CancelEventAsync(1, "Test reason");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            evt.IsActive.Should().BeFalse();
            _mockUnitOfWork.Verify(u => u.Events.Update(evt), Times.Once);
        }

        [Fact]
        public async Task CancelEventAsync_ShouldReturnFail_WhenEventNotFound()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.Events.GetByIdWithDetailsAsync(999))
                .ReturnsAsync((Event?)null);

            // Act
            var result = await _manager.CancelEventAsync(999, "Test reason");

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task CancelEventAsync_ShouldRefundRegistrations_WhenPriceGreaterThanZero()
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
                Price = 100,
                Capacity = 100,
                RegisteredCount = 2,
                IsActive = true,
                Registrations = new List<EventRegistration>
                {
                    new EventRegistration { Id = 1, UserId = "user1", EventId = 1, IsActive = true },
                    new EventRegistration { Id = 2, UserId = "user2", EventId = 1, IsActive = true },
                    new EventRegistration { Id = 3, UserId = "user3", EventId = 1, IsActive = false } // Inactive
                }
            };

            _mockUnitOfWork.Setup(u => u.Events.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(evt);
            _mockWalletService.Setup(w => w.RefundAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<ReferenceType>(), It.IsAny<int?>(), It.IsAny<string>()))
                .ReturnsAsync(new Response<WalletTransactionDto> { IsSuccessful = true });
            _mockUnitOfWork.Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _manager.CancelEventAsync(1, "Test reason");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            _mockWalletService.Verify(w => w.RefundAsync("user1", 100, ReferenceType.EventRegistration, 1, It.IsAny<string>()), Times.Once);
            _mockWalletService.Verify(w => w.RefundAsync("user2", 100, ReferenceType.EventRegistration, 2, It.IsAny<string>()), Times.Once);
            _mockWalletService.Verify(w => w.RefundAsync("user3", It.IsAny<decimal>(), It.IsAny<ReferenceType>(), It.IsAny<int?>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task JoinWaitlistAsync_ShouldReturnSuccess_WhenValid()
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
                RegisteredCount = 100,
                IsActive = true
            };

            _mockUnitOfWork.Setup(u => u.Events.GetByIdAsync(1))
                .ReturnsAsync(evt);
            _mockUnitOfWork.Setup(u => u.EventRegistrations.IsUserRegisteredAsync(1, "user1"))
                .ReturnsAsync(false);
            _mockUnitOfWork.Setup(u => u.EventWaitlists.IsUserInWaitlistAsync(1, "user1"))
                .ReturnsAsync(false);
            _mockUnitOfWork.Setup(u => u.EventWaitlists.GetMaxPositionAsync(1))
                .ReturnsAsync(5);
            _mockUnitOfWork.Setup(u => u.EventWaitlists.AddAsync(It.IsAny<EventWaitlist>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _manager.JoinWaitlistAsync("user1", 1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(201);
            result.Data.QueuePosition.Should().Be(6);
            result.Data.EventId.Should().Be(1);
            result.Data.UserId.Should().Be("user1");
        }

        [Fact]
        public async Task JoinWaitlistAsync_ShouldReturnFail_WhenEventNotFound()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.Events.GetByIdAsync(999))
                .ReturnsAsync((Event?)null);

            // Act
            var result = await _manager.JoinWaitlistAsync("user1", 999);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task JoinWaitlistAsync_ShouldReturnFail_WhenEventInactive()
        {
            // Arrange
            var evt = new Event { Id = 1, Title = "Test Event", IsActive = false };

            _mockUnitOfWork.Setup(u => u.Events.GetByIdAsync(1))
                .ReturnsAsync(evt);

            // Act
            var result = await _manager.JoinWaitlistAsync("user1", 1);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task JoinWaitlistAsync_ShouldReturnFail_WhenUserAlreadyRegistered()
        {
            // Arrange
            var evt = new Event { Id = 1, Title = "Test Event", IsActive = true };

            _mockUnitOfWork.Setup(u => u.Events.GetByIdAsync(1))
                .ReturnsAsync(evt);
            _mockUnitOfWork.Setup(u => u.EventRegistrations.IsUserRegisteredAsync(1, "user1"))
                .ReturnsAsync(true);

            // Act
            var result = await _manager.JoinWaitlistAsync("user1", 1);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task JoinWaitlistAsync_ShouldReturnFail_WhenUserAlreadyInWaitlist()
        {
            // Arrange
            var evt = new Event { Id = 1, Title = "Test Event", IsActive = true };

            _mockUnitOfWork.Setup(u => u.Events.GetByIdAsync(1))
                .ReturnsAsync(evt);
            _mockUnitOfWork.Setup(u => u.EventRegistrations.IsUserRegisteredAsync(1, "user1"))
                .ReturnsAsync(false);
            _mockUnitOfWork.Setup(u => u.EventWaitlists.IsUserInWaitlistAsync(1, "user1"))
                .ReturnsAsync(true);

            // Act
            var result = await _manager.JoinWaitlistAsync("user1", 1);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task LeaveWaitlistAsync_ShouldReturnSuccess_WhenValid()
        {
            // Arrange
            var waitlist = new EventWaitlist
            {
                Id = 1,
                EventId = 1,
                UserId = "user1",
                QueuePosition = 1,
                IsActive = true,
                AddedAt = DateTime.UtcNow
            };

            _mockUnitOfWork.Setup(u => u.EventWaitlists.GetByEventAndUserAsync(1, "user1"))
                .ReturnsAsync(waitlist);
            _mockUnitOfWork.Setup(u => u.EventWaitlists.GetByEventIdAsync(1))
                .ReturnsAsync(new List<EventWaitlist>());
            _mockUnitOfWork.Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _manager.LeaveWaitlistAsync("user1", 1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            waitlist.IsActive.Should().BeFalse();
            _mockUnitOfWork.Verify(u => u.EventWaitlists.Update(waitlist), Times.Once);
        }

        [Fact]
        public async Task LeaveWaitlistAsync_ShouldReturnFail_WhenNotInWaitlist()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.EventWaitlists.GetByEventAndUserAsync(1, "user1"))
                .ReturnsAsync((EventWaitlist?)null);

            // Act
            var result = await _manager.LeaveWaitlistAsync("user1", 1);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task LeaveWaitlistAsync_ShouldUpdatePositions_AfterLeaving()
        {
            // Arrange
            var waitlist = new EventWaitlist
            {
                Id = 1,
                EventId = 1,
                UserId = "user1",
                QueuePosition = 1,
                IsActive = true,
                AddedAt = DateTime.UtcNow.AddMinutes(-10)
            };

            var otherWaitlist = new EventWaitlist
            {
                Id = 2,
                EventId = 1,
                UserId = "user2",
                QueuePosition = 2,
                IsActive = true,
                AddedAt = DateTime.UtcNow.AddMinutes(-5)
            };

            _mockUnitOfWork.Setup(u => u.EventWaitlists.GetByEventAndUserAsync(1, "user1"))
                .ReturnsAsync(waitlist);
            _mockUnitOfWork.Setup(u => u.EventWaitlists.GetByEventIdAsync(1))
                .ReturnsAsync(new List<EventWaitlist> { otherWaitlist });
            _mockUnitOfWork.Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _manager.LeaveWaitlistAsync("user1", 1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            _mockUnitOfWork.Verify(u => u.EventWaitlists.Update(It.IsAny<EventWaitlist>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task CheckInAsync_ShouldReturnSuccess_WhenValid()
        {
            // Arrange
            var category = new EventCategory { Id = 1, Name = "Conference", IsActive = true };
            var user = new User { Id = "user1", UserName = "testuser" };
            var evt = new Event
            {
                Id = 1,
                Title = "Test Event",
                Description = "Test Description",
                CategoryId = 1,
                Category = category,
                StartDate = DateTime.UtcNow.AddHours(-2),
                EndDate = DateTime.UtcNow.AddHours(2),
                IsActive = true
            };

            var registration = new EventRegistration
            {
                Id = 1,
                EventId = 1,
                Event = evt,
                UserId = "user1",
                User = user,
                QRCode = "QR-123",
                CheckedIn = false,
                IsActive = true
            };

            _mockUnitOfWork.Setup(u => u.EventRegistrations.GetByQRCodeAsync("QR-123"))
                .ReturnsAsync(registration);
            _mockUnitOfWork.Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _manager.CheckInAsync("QR-123");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.IsValid.Should().BeTrue();
            result.Data.Message.Should().Contain("başarılı");
            registration.CheckedIn.Should().BeTrue();
            _mockUnitOfWork.Verify(u => u.EventRegistrations.Update(registration), Times.Once);
        }

        [Fact]
        public async Task CheckInAsync_ShouldReturnInvalid_WhenQRCodeNotFound()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.EventRegistrations.GetByQRCodeAsync("INVALID"))
                .ReturnsAsync((EventRegistration?)null);

            // Act
            var result = await _manager.CheckInAsync("INVALID");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.IsValid.Should().BeFalse();
            result.Data.Message.Should().Contain("Geçersiz");
        }

        [Fact]
        public async Task CheckInAsync_ShouldReturnInvalid_WhenRegistrationInactive()
        {
            // Arrange
            var registration = new EventRegistration
            {
                Id = 1,
                EventId = 1,
                UserId = "user1",
                QRCode = "QR-123",
                CheckedIn = false,
                IsActive = false
            };

            _mockUnitOfWork.Setup(u => u.EventRegistrations.GetByQRCodeAsync("QR-123"))
                .ReturnsAsync(registration);

            // Act
            var result = await _manager.CheckInAsync("QR-123");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.IsValid.Should().BeFalse();
            result.Data.Message.Should().Contain("Geçersiz");
        }

        [Fact]
        public async Task CheckInAsync_ShouldReturnInvalid_WhenAlreadyCheckedIn()
        {
            // Arrange
            var category = new EventCategory { Id = 1, Name = "Conference", IsActive = true };
            var user = new User { Id = "user1", UserName = "testuser" };
            var evt = new Event
            {
                Id = 1,
                Title = "Test Event",
                CategoryId = 1,
                Category = category,
                IsActive = true
            };

            var registration = new EventRegistration
            {
                Id = 1,
                EventId = 1,
                Event = evt,
                UserId = "user1",
                User = user,
                QRCode = "QR-123",
                CheckedIn = true,
                IsActive = true
            };

            _mockUnitOfWork.Setup(u => u.EventRegistrations.GetByQRCodeAsync("QR-123"))
                .ReturnsAsync(registration);

            // Act
            var result = await _manager.CheckInAsync("QR-123");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.IsValid.Should().BeFalse();
            result.Data.Message.Should().Contain("zaten yapılmış");
        }

        [Fact]
        public async Task CheckInAsync_ShouldReturnInvalid_WhenOutsideEventTime()
        {
            // Arrange
            var category = new EventCategory { Id = 1, Name = "Conference", IsActive = true };
            var user = new User { Id = "user1", UserName = "testuser" };
            var evt = new Event
            {
                Id = 1,
                Title = "Test Event",
                CategoryId = 1,
                Category = category,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(1).AddHours(2),
                IsActive = true
            };

            var registration = new EventRegistration
            {
                Id = 1,
                EventId = 1,
                Event = evt,
                UserId = "user1",
                User = user,
                QRCode = "QR-123",
                CheckedIn = false,
                IsActive = true
            };

            _mockUnitOfWork.Setup(u => u.EventRegistrations.GetByQRCodeAsync("QR-123"))
                .ReturnsAsync(registration);

            // Act
            var result = await _manager.CheckInAsync("QR-123");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.IsValid.Should().BeFalse();
            result.Data.Message.Should().Contain("saati dışında");
        }

        [Fact]
        public async Task CheckInAsync_ShouldReturnInvalid_WhenTooEarly()
        {
            // Arrange
            var category = new EventCategory { Id = 1, Name = "Conference", IsActive = true };
            var user = new User { Id = "user1", UserName = "testuser" };
            var evt = new Event
            {
                Id = 1,
                Title = "Test Event",
                CategoryId = 1,
                Category = category,
                StartDate = DateTime.UtcNow.AddHours(2),
                EndDate = DateTime.UtcNow.AddHours(4),
                IsActive = true
            };

            var registration = new EventRegistration
            {
                Id = 1,
                EventId = 1,
                Event = evt,
                UserId = "user1",
                User = user,
                QRCode = "QR-123",
                CheckedIn = false,
                IsActive = true
            };

            _mockUnitOfWork.Setup(u => u.EventRegistrations.GetByQRCodeAsync("QR-123"))
                .ReturnsAsync(registration);

            // Act
            var result = await _manager.CheckInAsync("QR-123");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.IsValid.Should().BeFalse();
            result.Data.Message.Should().Contain("saati dışında");
        }

        [Fact]
        public async Task GetEventRegistrationsAsync_ShouldReturnRegistrations()
        {
            // Arrange
            var category = new EventCategory { Id = 1, Name = "Conference", IsActive = true };
            var user = new User { Id = "user1", UserName = "testuser" };
            var evt = new Event
            {
                Id = 1,
                Title = "Test Event",
                CategoryId = 1,
                Category = category,
                IsActive = true
            };

            var registrations = new List<EventRegistration>
            {
                new EventRegistration
                {
                    Id = 1,
                    EventId = 1,
                    Event = evt,
                    UserId = "user1",
                    User = user,
                    RegistrationDate = DateTime.UtcNow,
                    QRCode = "QR-123",
                    CheckedIn = false,
                    IsActive = true
                },
                new EventRegistration
                {
                    Id = 2,
                    EventId = 1,
                    Event = evt,
                    UserId = "user2",
                    RegistrationDate = DateTime.UtcNow,
                    QRCode = "QR-456",
                    CheckedIn = true,
                    CheckedInAt = DateTime.UtcNow,
                    IsActive = true
                }
            };

            _mockUnitOfWork.Setup(u => u.EventRegistrations.GetByEventIdAsync(1))
                .ReturnsAsync(registrations);

            // Act
            var result = await _manager.GetEventRegistrationsAsync(1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Data[0].Id.Should().Be(1);
            result.Data[0].EventTitle.Should().Be("Test Event");
            result.Data[0].UserName.Should().Be("testuser");
            result.Data[1].CheckedIn.Should().BeTrue();
        }
    }
}

