using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Scheduling;
using SMARTCAMPUS.EntityLayer.Enums;
using System.Security.Claims;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class ClassroomReservationsControllerTests
    {
        private readonly Mock<IClassroomReservationService> _mockReservationService;
        private readonly ClassroomReservationsController _controller;

        public ClassroomReservationsControllerTests()
        {
            _mockReservationService = new Mock<IClassroomReservationService>();
            _controller = new ClassroomReservationsController(_mockReservationService.Object);
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

        #region GetMyReservations Tests

        [Fact]
        public async Task GetMyReservations_WithValidUser_ShouldReturnReservations()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var reservations = new List<ClassroomReservationDto>
            {
                new ClassroomReservationDto
                {
                    Id = 1,
                    RequestedByUserId = userId,
                    ClassroomId = 1,
                    ReservationDate = DateTime.Today.AddDays(1),
                    StartTime = TimeSpan.FromHours(9),
                    EndTime = TimeSpan.FromHours(11),
                    Purpose = "Ders",
                    Status = ReservationStatus.Pending
                }
            };

            var response = Response<List<ClassroomReservationDto>>.Success(reservations, 200);
            _mockReservationService.Setup(s => s.GetMyReservationsAsync(userId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetMyReservations();

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(response);
        }

        #endregion

        #region Create Reservation Tests

        [Fact]
        public async Task Create_WithValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            var userId = "faculty1";
            SetupHttpContext(userId, "Faculty");

            var createDto = new ClassroomReservationCreateDto
            {
                ClassroomId = 1,
                ReservationDate = DateTime.Today.AddDays(1),
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(11),
                Purpose = "Ders"
            };

            var reservationDto = new ClassroomReservationDto
            {
                Id = 1,
                RequestedByUserId = userId,
                ClassroomId = 1,
                ReservationDate = createDto.ReservationDate,
                StartTime = createDto.StartTime,
                EndTime = createDto.EndTime,
                Purpose = createDto.Purpose,
                Status = ReservationStatus.Pending
            };

            var response = Response<ClassroomReservationDto>.Success(reservationDto, 201);
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
        public async Task Create_WithTimeConflict_ShouldReturnError()
        {
            // Arrange
            var userId = "faculty1";
            SetupHttpContext(userId, "Faculty");

            var createDto = new ClassroomReservationCreateDto
            {
                ClassroomId = 1,
                ReservationDate = DateTime.Today.AddDays(1),
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(11),
                Purpose = "Ders"
            };

            var response = Response<ClassroomReservationDto>.Fail("Bu saatte sınıf rezerve edilmiş", 400);
            _mockReservationService.Setup(s => s.CreateReservationAsync(userId, createDto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task Create_WithUnauthorizedUser_ShouldReturnUnauthorized()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            var createDto = new ClassroomReservationCreateDto
            {
                ClassroomId = 1,
                ReservationDate = DateTime.Today.AddDays(1),
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(11),
                Purpose = "Ders"
            };

            // Act
            var result = await _controller.Create(createDto);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        #endregion

        #region Cancel Reservation Tests

        [Fact]
        public async Task Cancel_WithValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            var userId = "faculty1";
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

        #endregion

        #region Admin Operations Tests

        [Fact]
        public async Task Approve_WithAdminRole_ShouldReturnSuccess()
        {
            // Arrange
            var adminUserId = "admin1";
            SetupHttpContext(adminUserId, "Admin");

            var reservationId = 1;
            var approvalDto = new SMARTCAMPUS.API.Controllers.ReservationApprovalDto { Notes = null };
            var response = Response<NoDataDto>.Success(200);
            _mockReservationService.Setup(s => s.ApproveReservationAsync(adminUserId, reservationId, null))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Approve(reservationId, approvalDto);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Reject_WithAdminRole_ShouldReturnSuccess()
        {
            // Arrange
            var adminUserId = "admin1";
            SetupHttpContext(adminUserId, "Admin");

            var reservationId = 1;
            var reason = "Uygun değil";
            var rejectionDto = new SMARTCAMPUS.API.Controllers.ReservationRejectionDto { Reason = reason };
            var response = Response<NoDataDto>.Success(200);
            _mockReservationService.Setup(s => s.RejectReservationAsync(adminUserId, reservationId, reason))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Reject(reservationId, rejectionDto);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetPending_WithAdminRole_ShouldReturnReservations()
        {
            // Arrange
            var adminUserId = "admin1";
            SetupHttpContext(adminUserId, "Admin");

            var reservations = new List<ClassroomReservationDto>
            {
                new ClassroomReservationDto
                {
                    Id = 1,
                    RequestedByUserId = "faculty1",
                    ClassroomId = 1,
                    ReservationDate = DateTime.Today.AddDays(1),
                    Status = ReservationStatus.Pending
                }
            };

            var response = Response<List<ClassroomReservationDto>>.Success(reservations, 200);
            _mockReservationService.Setup(s => s.GetPendingReservationsAsync())
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetPending();

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
        }

        #endregion
    }
}

