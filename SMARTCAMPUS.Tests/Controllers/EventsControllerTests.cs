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

        #region Create Tests

        [Fact]
        public async Task Create_WithValidDto_ShouldReturnSuccess()
        {
            // Arrange
            var userId = "organizer1";
            SetupHttpContext(userId, "EventOrganizer");

            var dto = new EventCreateDto
            {
                Title = "New Event",
                Description = "Event Description",
                CategoryId = 1,
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(1).AddHours(2),
                Location = "Main Hall",
                Price = 0,
                Capacity = 100
            };

            var eventDto = new EventDto
            {
                Id = 1,
                Title = dto.Title,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Location = dto.Location,
                Price = dto.Price,
                Capacity = dto.Capacity
            };

            var response = Response<EventDto>.Success(eventDto, 201);
            _mockEventService.Setup(s => s.CreateEventAsync(userId, dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task Create_WithNullUserId_ShouldReturnUnauthorized()
        {
            // Arrange
            SetupHttpContext(null); // No user ID

            var dto = new EventCreateDto
            {
                Title = "New Event",
                CategoryId = 1
            };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorizedResult.Value.Should().Be("User ID not found");
        }

        [Fact]
        public async Task Create_WithInvalidDto_ShouldReturnError()
        {
            // Arrange
            var userId = "organizer1";
            SetupHttpContext(userId, "EventOrganizer");

            var dto = new EventCreateDto
            {
                Title = "New Event",
                CategoryId = 1,
                StartDate = DateTime.UtcNow.AddDays(-1), // Past date
                EndDate = DateTime.UtcNow.AddDays(-1).AddHours(2)
            };

            var response = Response<EventDto>.Fail("Geçmiş tarih için etkinlik oluşturulamaz", 400);
            _mockEventService.Setup(s => s.CreateEventAsync(userId, dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(400);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_WithValidDto_ShouldReturnSuccess()
        {
            // Arrange
            var userId = "organizer1";
            SetupHttpContext(userId, "EventOrganizer");

            var eventId = 1;
            var dto = new EventUpdateDto
            {
                Title = "Updated Event Title"
            };

            var eventDto = new EventDto
            {
                Id = eventId,
                Title = dto.Title
            };

            var response = Response<EventDto>.Success(eventDto, 200);
            _mockEventService.Setup(s => s.UpdateEventAsync(userId, eventId, dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Update(eventId, dto);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Update_WithNullUserId_ShouldReturnUnauthorized()
        {
            // Arrange
            SetupHttpContext(null);

            var eventId = 1;
            var dto = new EventUpdateDto { Title = "Updated Title" };

            // Act
            var result = await _controller.Update(eventId, dto);

            // Assert
            var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorizedResult.Value.Should().Be("User ID not found");
        }

        [Fact]
        public async Task Update_WithNotFound_ShouldReturn404()
        {
            // Arrange
            var userId = "organizer1";
            SetupHttpContext(userId, "EventOrganizer");

            var eventId = 999;
            var dto = new EventUpdateDto { Title = "Updated Title" };

            var response = Response<EventDto>.Fail("Etkinlik bulunamadı", 404);
            _mockEventService.Setup(s => s.UpdateEventAsync(userId, eventId, dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Update(eventId, dto);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(404);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithValidId_ShouldReturnSuccess()
        {
            // Arrange
            var userId = "organizer1";
            SetupHttpContext(userId, "EventOrganizer");

            var eventId = 1;
            var response = Response<NoDataDto>.Success(200);
            _mockEventService.Setup(s => s.DeleteEventAsync(userId, eventId, false))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Delete(eventId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Delete_WithForceTrue_ShouldReturnSuccess()
        {
            // Arrange
            var userId = "organizer1";
            SetupHttpContext(userId, "EventOrganizer");

            var eventId = 1;
            var response = Response<NoDataDto>.Success(200);
            _mockEventService.Setup(s => s.DeleteEventAsync(userId, eventId, true))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Delete(eventId, true);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Delete_WithNullUserId_ShouldReturnUnauthorized()
        {
            // Arrange
            SetupHttpContext(null);

            var eventId = 1;

            // Act
            var result = await _controller.Delete(eventId);

            // Assert
            var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorizedResult.Value.Should().Be("User ID not found");
        }

        [Fact]
        public async Task Delete_WithNotFound_ShouldReturn404()
        {
            // Arrange
            var userId = "organizer1";
            SetupHttpContext(userId, "EventOrganizer");

            var eventId = 999;
            var response = Response<NoDataDto>.Fail("Etkinlik bulunamadı", 404);
            _mockEventService.Setup(s => s.DeleteEventAsync(userId, eventId, false))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Delete(eventId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(404);
        }

        #endregion

        #region Publish Tests

        [Fact]
        public async Task Publish_WithValidId_ShouldReturnSuccess()
        {
            // Arrange
            var userId = "admin1";
            SetupHttpContext(userId, "Admin");

            var eventId = 1;
            var response = Response<NoDataDto>.Success(200);
            _mockEventService.Setup(s => s.PublishEventAsync(eventId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Publish(eventId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Publish_WithNotFound_ShouldReturn404()
        {
            // Arrange
            var userId = "admin1";
            SetupHttpContext(userId, "Admin");

            var eventId = 999;
            var response = Response<NoDataDto>.Fail("Etkinlik bulunamadı", 404);
            _mockEventService.Setup(s => s.PublishEventAsync(eventId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Publish(eventId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(404);
        }

        #endregion

        #region Cancel Tests

        [Fact]
        public async Task Cancel_WithValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            var userId = "admin1";
            SetupHttpContext(userId, "Admin");

            var eventId = 1;
            var cancelDto = new EventCancelDto { Reason = "Event cancelled due to weather" };
            var response = Response<NoDataDto>.Success(200);
            _mockEventService.Setup(s => s.CancelEventAsync(eventId, cancelDto.Reason))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Cancel(eventId, cancelDto);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Cancel_WithNotFound_ShouldReturn404()
        {
            // Arrange
            var userId = "admin1";
            SetupHttpContext(userId, "Admin");

            var eventId = 999;
            var cancelDto = new EventCancelDto { Reason = "Cancellation reason" };
            var response = Response<NoDataDto>.Fail("Etkinlik bulunamadı", 404);
            _mockEventService.Setup(s => s.CancelEventAsync(eventId, cancelDto.Reason))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Cancel(eventId, cancelDto);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(404);
        }

        #endregion

        #region GetRegistrations Tests

        [Fact]
        public async Task GetRegistrations_WithValidId_ShouldReturnRegistrations()
        {
            // Arrange
            var userId = "organizer1";
            SetupHttpContext(userId, "EventOrganizer");

            var eventId = 1;
            var registrations = new List<EventRegistrationDto>
            {
                new EventRegistrationDto
                {
                    Id = 1,
                    EventId = eventId,
                    UserId = "user1",
                    RegistrationDate = DateTime.UtcNow,
                    QRCode = "EVENT-1-ABC123",
                    CheckedIn = false
                },
                new EventRegistrationDto
                {
                    Id = 2,
                    EventId = eventId,
                    UserId = "user2",
                    RegistrationDate = DateTime.UtcNow,
                    QRCode = "EVENT-1-DEF456",
                    CheckedIn = true
                }
            };

            var response = Response<List<EventRegistrationDto>>.Success(registrations, 200);
            _mockEventService.Setup(s => s.GetEventRegistrationsAsync(eventId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetRegistrations(eventId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
            objectResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task GetRegistrations_WithEmptyRegistrations_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = "organizer1";
            SetupHttpContext(userId, "EventOrganizer");

            var eventId = 1;
            var registrations = new List<EventRegistrationDto>();
            var response = Response<List<EventRegistrationDto>>.Success(registrations, 200);
            _mockEventService.Setup(s => s.GetEventRegistrationsAsync(eventId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.GetRegistrations(eventId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
        }

        #endregion

        #region JoinWaitlist Tests

        [Fact]
        public async Task JoinWaitlist_WithValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var eventId = 1;
            var waitlistDto = new EventWaitlistDto
            {
                Id = 1,
                EventId = eventId,
                UserId = userId,
                QueuePosition = 1,
                AddedAt = DateTime.UtcNow
            };

            var response = Response<EventWaitlistDto>.Success(waitlistDto, 201);
            _mockEventService.Setup(s => s.JoinWaitlistAsync(userId, eventId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.JoinWaitlist(eventId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task JoinWaitlist_WithNullUserId_ShouldReturnUnauthorized()
        {
            // Arrange
            SetupHttpContext(null);

            var eventId = 1;

            // Act
            var result = await _controller.JoinWaitlist(eventId);

            // Assert
            var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorizedResult.Value.Should().Be("User ID not found");
        }

        [Fact]
        public async Task JoinWaitlist_WithAlreadyRegistered_ShouldReturnError()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var eventId = 1;
            var response = Response<EventWaitlistDto>.Fail("Bu etkinliğe zaten kayıtlısınız", 400);
            _mockEventService.Setup(s => s.JoinWaitlistAsync(userId, eventId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.JoinWaitlist(eventId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task JoinWaitlist_WithAlreadyInWaitlist_ShouldReturnError()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var eventId = 1;
            var response = Response<EventWaitlistDto>.Fail("Zaten bekleme listesindesiniz", 400);
            _mockEventService.Setup(s => s.JoinWaitlistAsync(userId, eventId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.JoinWaitlist(eventId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task JoinWaitlist_WithEventNotFound_ShouldReturn404()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var eventId = 999;
            var response = Response<EventWaitlistDto>.Fail("Etkinlik bulunamadı", 404);
            _mockEventService.Setup(s => s.JoinWaitlistAsync(userId, eventId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.JoinWaitlist(eventId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(404);
        }

        #endregion

        #region LeaveWaitlist Tests

        [Fact]
        public async Task LeaveWaitlist_WithValidRequest_ShouldReturnSuccess()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var eventId = 1;
            var response = Response<NoDataDto>.Success(200);
            _mockEventService.Setup(s => s.LeaveWaitlistAsync(userId, eventId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.LeaveWaitlist(eventId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task LeaveWaitlist_WithNullUserId_ShouldReturnUnauthorized()
        {
            // Arrange
            SetupHttpContext(null);

            var eventId = 1;

            // Act
            var result = await _controller.LeaveWaitlist(eventId);

            // Assert
            var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
            unauthorizedResult.Value.Should().Be("User ID not found");
        }

        [Fact]
        public async Task LeaveWaitlist_WithNotInWaitlist_ShouldReturn404()
        {
            // Arrange
            var userId = "user1";
            SetupHttpContext(userId);

            var eventId = 1;
            var response = Response<NoDataDto>.Fail("Bekleme listesinde değilsiniz", 404);
            _mockEventService.Setup(s => s.LeaveWaitlistAsync(userId, eventId))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.LeaveWaitlist(eventId);

            // Assert
            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(404);
        }

        #endregion

        #region CheckIn Additional Tests

        [Fact]
        public async Task CheckIn_WithAlreadyCheckedIn_ShouldReturnError()
        {
            // Arrange
            var userId = "organizer1";
            SetupHttpContext(userId, "Admin");

            var qrCode = "EVENT-1-ABC123";
            var checkInResult = new EventCheckInResultDto
            {
                IsValid = false,
                Message = "Giriş zaten yapılmış"
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
            var resultValue = objectResult.Value.Should().BeOfType<Response<EventCheckInResultDto>>().Subject;
            resultValue.Data.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task CheckIn_WithOutsideEventTime_ShouldReturnError()
        {
            // Arrange
            var userId = "organizer1";
            SetupHttpContext(userId, "Admin");

            var qrCode = "EVENT-1-ABC123";
            var checkInResult = new EventCheckInResultDto
            {
                IsValid = false,
                Message = "Etkinlik saati dışında giriş yapılamaz"
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
            var resultValue = objectResult.Value.Should().BeOfType<Response<EventCheckInResultDto>>().Subject;
            resultValue.Data.IsValid.Should().BeFalse();
        }

        #endregion
    }
}

