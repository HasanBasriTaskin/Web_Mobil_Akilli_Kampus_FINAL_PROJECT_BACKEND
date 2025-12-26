using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Event;
using System.Security.Claims;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class EventsControllerTests
    {
        private readonly Mock<IEventService> _mockEventService;
        private readonly EventsController _controller;

        public EventsControllerTests()
        {
            _mockEventService = new Mock<IEventService>();
            _controller = new EventsController(_mockEventService.Object);
        }

        private void SetupHttpContext(string? userId = null, string? role = null)
        {
            var claims = new List<Claim>();

            if (!string.IsNullOrEmpty(userId))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            }

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

        #region GetEvents Tests

        [Fact]
        public async Task GetEvents_WithValidFilter_ShouldReturnEvents()
        {
            // Arrange
            SetupHttpContext();

            var filter = new EventFilterDto
            {
                CategoryId = 1,
                FromDate = DateTime.Today,
                ToDate = DateTime.Today.AddDays(30)
            };

            var events = new List<EventListDto>
            {
                new EventListDto
                {
                    Id = 1,
                    Title = "Tech Conference",
                    CategoryId = 1,
                    CategoryName = "Conference",
                    StartDate = DateTime.Today.AddDays(5),
                    EndDate = DateTime.Today.AddDays(5).AddHours(3),
                    Location = "Main Hall",
                    Capacity = 100,
                    RegisteredCount = 50
                }
            };

            var pagedResponse = new PagedResponse<EventListDto>(events, 1, 20, 1);

            var response = Response<PagedResponse<EventListDto>>.Success(pagedResponse, 200);
            _mockEventService.Setup(s => s.GetEventsAsync(filter, 1, 20))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetEvents(filter, 1, 20);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(response);
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_WithValidId_ShouldReturnEvent()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var eventId = 1;
            var eventDto = new EventDto
            {
                Id = eventId,
                Title = "Tech Conference",
                Description = "Annual tech conference",
                CategoryId = 1,
                CategoryName = "Conference",
                StartDate = DateTime.Today.AddDays(5),
                EndDate = DateTime.Today.AddDays(5).AddHours(3),
                Location = "Main Hall",
                Capacity = 100,
                RegisteredCount = 50,
                Price = 0
            };

            var response = Response<EventDto>.Success(eventDto, 200);
            _mockEventService.Setup(s => s.GetEventByIdAsync(eventId, userId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetById(eventId);

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

            var eventId = 999;
            var response = Response<EventDto>.Fail("Etkinlik bulunamadı", 404);
            _mockEventService.Setup(s => s.GetEventByIdAsync(eventId, userId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetById(eventId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(404);
        }

        #endregion

        #region Register Tests

        [Fact]
        public async Task Register_WithValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var eventId = 1;
            var registrationDto = new EventRegistrationDto
            {
                Id = 1,
                EventId = eventId,
                UserId = userId,
                RegistrationDate = DateTime.UtcNow,
                QRCode = "EVENT-1-ABC123",
                CheckedIn = false
            };

            var response = Response<EventRegistrationDto>.Success(registrationDto, 201);
            _mockEventService.Setup(s => s.RegisterAsync(userId, eventId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Register(eventId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(201);
            objectResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task Register_WithFullCapacity_ShouldReturnError()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var eventId = 1;
            var response = Response<EventRegistrationDto>.Fail("Etkinlik kapasitesi dolu", 400);
            _mockEventService.Setup(s => s.RegisterAsync(userId, eventId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Register(eventId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task Register_WithAlreadyRegistered_ShouldReturnError()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var eventId = 1;
            var response = Response<EventRegistrationDto>.Fail("Bu etkinliğe zaten kayıtlısınız", 400);
            _mockEventService.Setup(s => s.RegisterAsync(userId, eventId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Register(eventId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(400);
        }

        #endregion

        #region CancelRegistration Tests

        [Fact]
        public async Task CancelRegistration_WithValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var eventId = 1;
            var response = Response<NoDataDto>.Success(200);
            _mockEventService.Setup(s => s.CancelRegistrationAsync(userId, eventId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CancelRegistration(eventId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task CancelRegistration_WithNotRegistered_ShouldReturnError()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var eventId = 1;
            var response = Response<NoDataDto>.Fail("Bu etkinliğe kayıtlı değilsiniz", 404);
            _mockEventService.Setup(s => s.CancelRegistrationAsync(userId, eventId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.CancelRegistration(eventId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(404);
        }

        #endregion

        #region GetMyRegistrations Tests

        [Fact]
        public async Task GetMyRegistrations_WithValidUser_ShouldReturnRegistrations()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var registrations = new List<EventRegistrationDto>
            {
                new EventRegistrationDto
                {
                    Id = 1,
                    EventId = 1,
                    UserId = userId,
                    RegistrationDate = DateTime.UtcNow,
                    QRCode = "EVENT-1-ABC123",
                    CheckedIn = false
                }
            };

            var response = Response<List<EventRegistrationDto>>.Success(registrations, 200);
            _mockEventService.Setup(s => s.GetMyRegistrationsAsync(userId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetMyRegistrations();

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(response);
        }

        #endregion

        #region CheckIn Tests

        [Fact]
        public async Task CheckIn_WithValidQRCode_ShouldReturnSuccess()
        {
            // Arrange
            var userId = "organizer1";
            SetupHttpContext(userId, "Admin");

            var qrCode = "EVENT-1-ABC123";
            var checkInResult = new EventCheckInResultDto
            {
                IsValid = true,
                UserName = "Test User",
                EventTitle = "Tech Conference",
                Message = "Check-in successful"
            };

            var response = Response<EventCheckInResultDto>.Success(checkInResult, 200);
            _mockEventService.Setup(s => s.CheckInAsync(qrCode))
                .ReturnsAsync(response);

            var checkInDto = new SMARTCAMPUS.API.Controllers.EventCheckInDto { QRCode = qrCode };

            // Act
            var result = await _controller.CheckIn(checkInDto);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task CheckIn_WithInvalidQRCode_ShouldReturnError()
        {
            // Arrange
            var userId = "organizer1";
            SetupHttpContext(userId, "Admin");

            var qrCode = "INVALID-QR-CODE";
            var response = Response<EventCheckInResultDto>.Fail("Geçersiz QR kod", 400);
            _mockEventService.Setup(s => s.CheckInAsync(qrCode))
                .ReturnsAsync(response);

            var checkInDto = new SMARTCAMPUS.API.Controllers.EventCheckInDto { QRCode = qrCode };

            // Act
            var result = await _controller.CheckIn(checkInDto);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(400);
        }

        #endregion
    }
}

