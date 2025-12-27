using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Reservation;
using SMARTCAMPUS.EntityLayer.Enums;
using System.Security.Claims;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class MealReservationsControllerTests
    {
        private readonly Mock<IMealReservationService> _mockReservationService;
        private readonly MealReservationsController _controller;

        public MealReservationsControllerTests()
        {
            _mockReservationService = new Mock<IMealReservationService>();
            _controller = new MealReservationsController(_mockReservationService.Object);
        }

        private void SetupHttpContext(string userId, string? role = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            if (!string.IsNullOrEmpty(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        #region Create Reservation Tests

        [Fact]
        public async Task Create_WithValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var createDto = new MealReservationCreateDto
            {
                MenuId = 1,
                CafeteriaId = 1,
                MealType = MealType.Lunch,
                Date = DateTime.Today.AddDays(1)
            };

            var reservationDto = new MealReservationDto
            {
                Id = 1,
                UserId = userId,
                MenuId = 1,
                CafeteriaId = 1,
                MealType = MealType.Lunch,
                Date = createDto.Date,
                QRCode = "MEAL-1-ABC123",
                Status = MealReservationStatus.Reserved
            };

            var response = Response<MealReservationDto>.Success(reservationDto, 201);
            _mockReservationService.Setup(s => s.CreateReservationAsync(userId, createDto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(201);
            objectResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task Create_WithUnauthorizedUser_ShouldReturnUnauthorized()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            var createDto = new MealReservationCreateDto
            {
                MenuId = 1,
                CafeteriaId = 1,
                MealType = MealType.Lunch,
                Date = DateTime.Today.AddDays(1)
            };

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task Create_WithQuotaExceeded_ShouldReturnError()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var createDto = new MealReservationCreateDto
            {
                MenuId = 1,
                CafeteriaId = 1,
                MealType = MealType.Lunch,
                Date = DateTime.Today.AddDays(1)
            };

            var response = Response<MealReservationDto>.Fail("Günlük kotayı aştınız (max 2 öğün)", 400);
            _mockReservationService.Setup(s => s.CreateReservationAsync(userId, createDto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task Create_WithInsufficientBalance_ShouldReturnError()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var createDto = new MealReservationCreateDto
            {
                MenuId = 1,
                CafeteriaId = 1,
                MealType = MealType.Dinner,
                Date = DateTime.Today.AddDays(1)
            };

            var response = Response<MealReservationDto>.Fail("Yetersiz bakiye", 400);
            _mockReservationService.Setup(s => s.CreateReservationAsync(userId, createDto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(400);
        }

        #endregion

        #region GetMyReservations Tests

        [Fact]
        public async Task GetMyReservations_WithValidUser_ShouldReturnReservations()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var reservations = new List<MealReservationListDto>
            {
                new MealReservationListDto
                {
                    Id = 1,
                    MealType = MealType.Lunch,
                    Date = DateTime.Today.AddDays(1),
                    Status = MealReservationStatus.Reserved,
                    QRCode = "MEAL-1-ABC123"
                }
            };

            var response = Response<List<MealReservationListDto>>.Success(reservations, 200);
            _mockReservationService.Setup(s => s.GetMyReservationsAsync(userId, null, null))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetMyReservations(null, null);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task GetMyReservations_WithDateFilter_ShouldReturnFilteredReservations()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var fromDate = DateTime.Today;
            var toDate = DateTime.Today.AddDays(7);

            var reservations = new List<MealReservationListDto>();
            var response = Response<List<MealReservationListDto>>.Success(reservations, 200);
            _mockReservationService.Setup(s => s.GetMyReservationsAsync(userId, fromDate, toDate))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetMyReservations(fromDate, toDate);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            _mockReservationService.Verify(s => s.GetMyReservationsAsync(userId, fromDate, toDate), Times.Once);
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_WithValidId_ShouldReturnReservation()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var reservationId = 1;
            var reservationDto = new MealReservationDto
            {
                Id = reservationId,
                UserId = userId,
                MenuId = 1,
                CafeteriaId = 1,
                MealType = MealType.Lunch,
                Date = DateTime.Today.AddDays(1),
                QRCode = "MEAL-1-ABC123",
                Status = MealReservationStatus.Reserved
            };

            var response = Response<MealReservationDto>.Success(reservationDto, 200);
            _mockReservationService.Setup(s => s.GetReservationByIdAsync(userId, reservationId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetById(reservationId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task GetById_WithNotFound_ShouldReturn404()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var reservationId = 999;
            var response = Response<MealReservationDto>.Fail("Rezervasyon bulunamadı", 404);
            _mockReservationService.Setup(s => s.GetReservationByIdAsync(userId, reservationId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetById(reservationId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(404);
        }

        #endregion

        #region Cancel Reservation Tests

        [Fact]
        public async Task Cancel_WithValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var reservationId = 1;
            var response = Response<NoDataDto>.Success(200);
            _mockReservationService.Setup(s => s.CancelReservationAsync(userId, reservationId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Cancel(reservationId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Cancel_WithTooLate_ShouldReturnError()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var reservationId = 1;
            var response = Response<NoDataDto>.Fail("Rezervasyon iptali için en az 2 saat önceden iptal etmelisiniz", 400);
            _mockReservationService.Setup(s => s.CancelReservationAsync(userId, reservationId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Cancel(reservationId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(400);
        }

        #endregion

        #region QR Code Operations Tests

        [Fact]
        public async Task Scan_WithValidQRCode_ShouldReturnSuccess()
        {
            // Arrange
            var userId = "staff1";
            SetupHttpContext(userId, "CafeteriaStaff");

            var qrCode = "MEAL-1-ABC123";
            var scanResult = new MealScanResultDto
            {
                ReservationId = 1,
                UserName = "Test User",
                MealType = MealType.Lunch,
                Date = DateTime.Today,
                IsValid = true
            };

            var response = Response<MealScanResultDto>.Success(scanResult, 200);
            _mockReservationService.Setup(s => s.ScanQRCodeAsync(qrCode))
                .ReturnsAsync(response);

            var scanDto = new SMARTCAMPUS.API.Controllers.MealScanDto { QRCode = qrCode };

            // Act
            var result = await _controller.Scan(scanDto);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Scan_WithInvalidQRCode_ShouldReturnError()
        {
            // Arrange
            var userId = "staff1";
            SetupHttpContext(userId, "CafeteriaStaff");

            var qrCode = "INVALID-QR-CODE";
            var response = Response<MealScanResultDto>.Fail("Geçersiz QR kod", 400);
            _mockReservationService.Setup(s => s.ScanQRCodeAsync(qrCode))
                .ReturnsAsync(response);

            var scanDto = new SMARTCAMPUS.API.Controllers.MealScanDto { QRCode = qrCode };

            // Act
            var result = await _controller.Scan(scanDto);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task GetByQR_WithValidQRCode_ShouldReturnReservation()
        {
            // Arrange
            var userId = "staff1";
            SetupHttpContext(userId, "CafeteriaStaff");

            var qrCode = "MEAL-1-ABC123";
            var reservationDto = new MealReservationDto
            {
                Id = 1,
                UserId = "user1",
                MenuId = 1,
                CafeteriaId = 1,
                MealType = MealType.Lunch,
                Date = DateTime.Today,
                QRCode = qrCode,
                Status = MealReservationStatus.Reserved
            };

            var response = Response<MealReservationDto>.Success(reservationDto, 200);
            _mockReservationService.Setup(s => s.GetReservationByQRAsync(qrCode))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetByQR(qrCode);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
        }

        #endregion

        #region Admin Operations Tests

        [Fact]
        public async Task GetByDate_WithAdminRole_ShouldReturnReservations()
        {
            // Arrange
            var userId = "admin1";
            SetupHttpContext(userId, "Admin");

            var date = DateTime.Today;
            var reservations = new List<MealReservationDto>
            {
                new MealReservationDto
                {
                    Id = 1,
                    UserId = "user1",
                    MenuId = 1,
                    CafeteriaId = 1,
                    MealType = MealType.Lunch,
                    Date = date,
                    Status = MealReservationStatus.Reserved
                }
            };

            var response = Response<List<MealReservationDto>>.Success(reservations, 200);
            _mockReservationService.Setup(s => s.GetReservationsByDateAsync(date, null, null))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetByDate(date, null, null);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task ExpireOld_WithAdminRole_ShouldReturnSuccess()
        {
            // Arrange
            var userId = "admin1";
            SetupHttpContext(userId, "Admin");

            var response = Response<NoDataDto>.Success(200);
            _mockReservationService.Setup(s => s.ExpireOldReservationsAsync())
                .ReturnsAsync(response);

            // Act
            var result = await _controller.ExpireOld();

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
        }

        #endregion
    }
}

