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

            var testDate = DateTime.UtcNow.Date.AddDays(1);
            _mockUnitOfWork.Setup(u => u.MealReservations.ExistsForUserDateMealTypeAsync("user1", testDate, MealType.Lunch))
                .ReturnsAsync(false);
            _mockUnitOfWork.Setup(u => u.MealMenus.GetByIdWithDetailsAsync(1))
                .ReturnsAsync(menu);
            _mockUnitOfWork.Setup(u => u.Students.GetByUserIdAsync("user1"))
                .ReturnsAsync(student);
            _mockUnitOfWork.Setup(u => u.MealReservations.GetDailyReservationCountAsync("user1", testDate))
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
                Date = DateTime.UtcNow.Date.AddDays(1),
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
    }
}

