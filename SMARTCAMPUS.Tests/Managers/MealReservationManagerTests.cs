using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Reservation;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class MealReservationManagerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IQRCodeService> _mockQRCodeService;
        private readonly Mock<IWalletService> _mockWalletService;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly MealReservationManager _manager;

        public MealReservationManagerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockQRCodeService = new Mock<IQRCodeService>();
            _mockWalletService = new Mock<IWalletService>();
            
            var store = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);

            _manager = new MealReservationManager(
                _mockUnitOfWork.Object,
                _mockQRCodeService.Object,
                _mockWalletService.Object,
                _mockUserManager.Object);
        }

        [Fact]
        public async Task CreateReservationAsync_ShouldReturnSuccess_ForScholarshipStudent()
        {
            // Arrange
            var student = new Student
            {
                Id = 1,
                UserId = "user1",
                HasScholarship = true,
                DailyMealQuota = 2
            };

            var cafeteria = new Cafeteria { Id = 1, Name = "Main", IsActive = true };
            var menu = new MealMenu
            {
                Id = 1,
                CafeteriaId = 1,
                Cafeteria = cafeteria,
                Date = DateTime.UtcNow.Date.AddDays(1),
                MealType = MealType.Lunch,
                Price = 25.00m,
                IsActive = true,
                IsPublished = true
            };

            var user = new User { Id = "user1", UserName = "testuser" };

            // Hafta içi bir gün seç (bugünden sonraki ilk hafta içi)
            var testDate = DateTime.UtcNow.Date.AddDays(1);
            while (testDate.DayOfWeek == DayOfWeek.Saturday || testDate.DayOfWeek == DayOfWeek.Sunday)
                testDate = testDate.AddDays(1);
            
            menu.Date = testDate;
            _mockUnitOfWork.Setup(u => u.MealReservations.ExistsForUserDateMealTypeAsync("user1", It.IsAny<DateTime>(), MealType.Lunch))
                .ReturnsAsync(false);
            _mockUnitOfWork.Setup(u => u.MealMenus.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(menu);
            _mockUnitOfWork.Setup(u => u.Students.GetByUserIdAsync("user1"))
                .ReturnsAsync(student);
            _mockUnitOfWork.Setup(u => u.Wallets.GetByUserIdAsync("user1"))
                .ReturnsAsync((Wallet?)null);
            _mockUnitOfWork.Setup(u => u.MealReservations.GetDailyReservationCountAsync("user1", It.IsAny<DateTime>()))
                .ReturnsAsync(0);
            _mockQRCodeService.Setup(q => q.GenerateQRCode("MEAL", 0))
                .Returns("QR-123");
            _mockQRCodeService.Setup(q => q.GenerateQRCode("MEAL", It.Is<int>(i => i > 0)))
                .Returns("QR-123");
            _mockUnitOfWork.Setup(u => u.MealReservations.AddAsync(It.IsAny<MealReservation>()))
                .Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask);
            _mockUserManager.Setup(u => u.FindByIdAsync("user1"))
                .ReturnsAsync(user);

            var dto = new MealReservationCreateDto
            {
                MenuId = 1,
                CafeteriaId = 1,
                Date = testDate,
                MealType = MealType.Lunch
            };

            // Act
            var result = await _manager.CreateReservationAsync("user1", dto);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task CreateReservationAsync_ShouldReturnFail_WhenPastDate()
        {
            // Arrange
            var dto = new MealReservationCreateDto
            {
                MenuId = 1,
                CafeteriaId = 1,
                Date = DateTime.UtcNow.Date.AddDays(-1),
                MealType = MealType.Lunch
            };

            // Act
            var result = await _manager.CreateReservationAsync("user1", dto);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task CreateReservationAsync_ShouldReturnFail_WhenWeekend()
        {
            // Arrange
            var nextSaturday = DateTime.UtcNow;
            while (nextSaturday.DayOfWeek != DayOfWeek.Saturday)
                nextSaturday = nextSaturday.AddDays(1);

            var dto = new MealReservationCreateDto
            {
                MenuId = 1,
                CafeteriaId = 1,
                Date = nextSaturday.Date,
                MealType = MealType.Lunch
            };

            // Act
            var result = await _manager.CreateReservationAsync("user1", dto);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task CancelReservationAsync_ShouldReturnSuccess_WhenValid()
        {
            // Arrange
            var cafeteria = new Cafeteria { Id = 1, Name = "Main" };
            var menu = new MealMenu
            {
                Id = 1,
                Cafeteria = cafeteria,
                Price = 25.00m
            };

            var reservation = new MealReservation
            {
                Id = 1,
                UserId = "user1",
                MenuId = 1,
                Menu = menu,
                Date = DateTime.UtcNow.Date.AddDays(1),
                MealType = MealType.Lunch,
                Status = MealReservationStatus.Reserved
            };

            _mockUnitOfWork.Setup(u => u.MealReservations.GetByUserAsync("user1", null, null))
                .ReturnsAsync(new List<MealReservation> { reservation });
            _mockUnitOfWork.Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _manager.CancelReservationAsync("user1", 1);

            // Assert
            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task GetMyReservationsAsync_ShouldReturnReservations()
        {
            // Arrange
            var cafeteria = new Cafeteria { Id = 1, Name = "Main" };
            var menu = new MealMenu
            {
                Id = 1,
                Cafeteria = cafeteria
            };

            var reservation = new MealReservation
            {
                Id = 1,
                UserId = "user1",
                MenuId = 1,
                Menu = menu,
                Date = DateTime.UtcNow.Date,
                MealType = MealType.Lunch,
                Status = MealReservationStatus.Reserved,
                QRCode = "QR-123"
            };

            _mockUnitOfWork.Setup(u => u.MealReservations.GetByUserAsync("user1", null, null))
                .ReturnsAsync(new List<MealReservation> { reservation });

            // Act
            var result = await _manager.GetMyReservationsAsync("user1");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task ScanQRCodeAsync_ShouldReturnSuccess_WhenValid()
        {
            // Arrange
            var cafeteria = new Cafeteria { Id = 1, Name = "Main" };
            var menu = new MealMenu
            {
                Id = 1,
                Cafeteria = cafeteria
            };

            var user = new User { Id = "user1", UserName = "testuser" };

            var reservation = new MealReservation
            {
                Id = 1,
                UserId = "user1",
                User = user,
                MenuId = 1,
                Menu = menu,
                Date = DateTime.UtcNow.Date,
                MealType = MealType.Lunch,
                Status = MealReservationStatus.Reserved,
                QRCode = "QR-123"
            };

            _mockUnitOfWork.Setup(u => u.MealReservations.GetByQRCodeAsync("QR-123"))
                .ReturnsAsync(reservation);
            _mockUnitOfWork.Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _manager.ScanQRCodeAsync("QR-123");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task ScanQRCodeAsync_ShouldReturnInvalid_WhenAlreadyUsed()
        {
            // Arrange
            var reservation = new MealReservation
            {
                Id = 1,
                Status = MealReservationStatus.Used,
                QRCode = "QR-123"
            };

            _mockUnitOfWork.Setup(u => u.MealReservations.GetByQRCodeAsync("QR-123"))
                .ReturnsAsync(reservation);

            // Act
            var result = await _manager.ScanQRCodeAsync("QR-123");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task ScanQRCodeAsync_ShouldReturnInvalid_WhenQRCodeNotFound()
        {
            // Arrange
            _mockUnitOfWork.Setup(u => u.MealReservations.GetByQRCodeAsync("INVALID-QR"))
                .ReturnsAsync((MealReservation?)null);

            // Act
            var result = await _manager.ScanQRCodeAsync("INVALID-QR");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.IsValid.Should().BeFalse();
            result.Data.Message.Should().Contain("Geçersiz");
        }

        [Fact]
        public async Task ScanQRCodeAsync_ShouldReturnInvalid_WhenCancelled()
        {
            // Arrange
            var reservation = new MealReservation
            {
                Id = 1,
                Status = MealReservationStatus.Cancelled,
                QRCode = "QR-123"
            };

            _mockUnitOfWork.Setup(u => u.MealReservations.GetByQRCodeAsync("QR-123"))
                .ReturnsAsync(reservation);

            // Act
            var result = await _manager.ScanQRCodeAsync("QR-123");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.IsValid.Should().BeFalse();
            result.Data.Message.Should().Contain("iptal");
        }

        [Fact]
        public async Task ScanQRCodeAsync_ShouldReturnInvalid_WhenExpired()
        {
            // Arrange
            var reservation = new MealReservation
            {
                Id = 1,
                Status = MealReservationStatus.Expired,
                QRCode = "QR-123"
            };

            _mockUnitOfWork.Setup(u => u.MealReservations.GetByQRCodeAsync("QR-123"))
                .ReturnsAsync(reservation);

            // Act
            var result = await _manager.ScanQRCodeAsync("QR-123");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.IsValid.Should().BeFalse();
            result.Data.Message.Should().Contain("süresi dolmuş");
        }

        [Fact]
        public async Task ScanQRCodeAsync_ShouldReturnInvalid_WhenWrongDate()
        {
            // Arrange
            var cafeteria = new Cafeteria { Id = 1, Name = "Main" };
            var menu = new MealMenu
            {
                Id = 1,
                Cafeteria = cafeteria
            };

            var user = new User { Id = "user1", UserName = "testuser" };

            var reservation = new MealReservation
            {
                Id = 1,
                UserId = "user1",
                User = user,
                MenuId = 1,
                Menu = menu,
                Date = DateTime.UtcNow.AddDays(-1).Date, // Yesterday
                MealType = MealType.Lunch,
                Status = MealReservationStatus.Reserved,
                QRCode = "QR-123"
            };

            _mockUnitOfWork.Setup(u => u.MealReservations.GetByQRCodeAsync("QR-123"))
                .ReturnsAsync(reservation);

            // Act
            var result = await _manager.ScanQRCodeAsync("QR-123");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.IsValid.Should().BeFalse();
            result.Data.Message.Should().Contain("tarihi için");
        }

        [Fact]
        public async Task ScanQRCodeAsync_ShouldUpdateStatusToUsed_WhenValid()
        {
            // Arrange
            var cafeteria = new Cafeteria { Id = 1, Name = "Main" };
            var menu = new MealMenu
            {
                Id = 1,
                Cafeteria = cafeteria
            };

            var user = new User { Id = "user1", UserName = "testuser" };

            var reservation = new MealReservation
            {
                Id = 1,
                UserId = "user1",
                User = user,
                MenuId = 1,
                Menu = menu,
                Date = DateTime.UtcNow.Date,
                MealType = MealType.Lunch,
                Status = MealReservationStatus.Reserved,
                QRCode = "QR-123"
            };

            _mockUnitOfWork.Setup(u => u.MealReservations.GetByQRCodeAsync("QR-123"))
                .ReturnsAsync(reservation);
            _mockUnitOfWork.Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _manager.ScanQRCodeAsync("QR-123");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.IsValid.Should().BeTrue();
            reservation.Status.Should().Be(MealReservationStatus.Used);
            reservation.UsedAt.Should().NotBeNull();
            _mockUnitOfWork.Verify(u => u.MealReservations.Update(reservation), Times.Once);
        }

        [Fact]
        public async Task GetReservationByIdAsync_ShouldReturnSuccess()
        {
            var reservation = new MealReservation
            {
                Id = 1,
                UserId = "user1",
                MenuId = 1,
                Menu = new MealMenu { Cafeteria = new Cafeteria { Name = "Main" } },
                Status = MealReservationStatus.Reserved
            };
            var user = new User { Id = "user1", UserName = "testuser" };

            _mockUnitOfWork.Setup(u => u.MealReservations.GetByUserAsync("user1", null, null))
                .ReturnsAsync(new List<MealReservation> { reservation });
            _mockUserManager.Setup(u => u.FindByIdAsync("user1"))
                .ReturnsAsync(user);

            var result = await _manager.GetReservationByIdAsync("user1", 1);

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task GetReservationByQRAsync_ShouldReturnSuccess()
        {
            var reservation = new MealReservation
            {
                Id = 1,
                QRCode = "QR-123",
                Menu = new MealMenu { Cafeteria = new Cafeteria { Name = "Main" } }
            };

            _mockUnitOfWork.Setup(u => u.MealReservations.GetByQRCodeAsync("QR-123"))
                .ReturnsAsync(reservation);

            var result = await _manager.GetReservationByQRAsync("QR-123");

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task GetReservationsByDateAsync_ShouldReturnSuccess()
        {
            var reservations = new List<MealReservation>
            {
                new MealReservation { Id = 1, Date = DateTime.Today, Menu = new MealMenu { Cafeteria = new Cafeteria { Name = "Main" } } }
            };

            _mockUnitOfWork.Setup(u => u.MealReservations.GetByDateAsync(DateTime.Today, null, null))
                .ReturnsAsync(reservations);

            var result = await _manager.GetReservationsByDateAsync(DateTime.Today);

            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task ExpireOldReservationsAsync_ShouldReturnSuccess()
        {
            _mockUnitOfWork.Setup(u => u.MealReservations.GetExpiredReservationsAsync())
                .ReturnsAsync(new List<MealReservation>());
            _mockUnitOfWork.Setup(u => u.CommitAsync())
                .Returns(Task.CompletedTask);

            var result = await _manager.ExpireOldReservationsAsync();

            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task GetMyReservationsAsync_ShouldFilterByDateRange()
        {
            var reservations = new List<MealReservation>
            {
                new MealReservation { Id = 1, UserId = "user1", Menu = new MealMenu { Cafeteria = new Cafeteria { Name = "Main" } } }
            };

            var fromDate = DateTime.Today;
            var toDate = DateTime.Today.AddDays(7);
            _mockUnitOfWork.Setup(u => u.MealReservations.GetByUserAsync("user1", fromDate, toDate))
                .ReturnsAsync(reservations);

            var result = await _manager.GetMyReservationsAsync("user1", fromDate, toDate);

            result.IsSuccessful.Should().BeTrue();
        }
    }
}

