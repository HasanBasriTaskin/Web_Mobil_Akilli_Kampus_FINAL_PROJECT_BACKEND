using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Attendance;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class AttendanceControllerTests
    {
        private readonly Mock<IAttendanceService> _mockService;
        private readonly AttendanceController _controller;

        public AttendanceControllerTests()
        {
            _mockService = new Mock<IAttendanceService>();
            _controller = new AttendanceController(_mockService.Object);

            // Setup User Claims
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("StudentId", "1"),
                new Claim("FacultyId", "2"),
                new Claim(ClaimTypes.Role, "Student")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task CreateSession_ShouldReturnStatusCode()
        {
            // Arrange
            var dto = new CreateSessionDto();
            var response = Response<AttendanceSessionDto>.Success(new AttendanceSessionDto(), 201);
            _mockService.Setup(x => x.CreateSessionAsync(2, dto)).ReturnsAsync(response);

            // Act
            var result = await _controller.CreateSession(dto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task CreateSession_ShouldReturnUnauthorized_WhenFacultyIdMissing()
        {
            // Arrange
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var result = await _controller.CreateSession(new CreateSessionDto()) as UnauthorizedObjectResult;

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetSession_ShouldReturnStatusCode()
        {
            // Arrange
            var response = Response<AttendanceSessionDto>.Success(new AttendanceSessionDto(), 200);
            _mockService.Setup(x => x.GetSessionByIdAsync(1)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetSession(1) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task CheckIn_ShouldReturnStatusCode()
        {
            // Arrange
            var dto = new CheckInDto();
            var response = Response<CheckInResultDto>.Success(new CheckInResultDto(), 200);
            _mockService.Setup(x => x.CheckInAsync(1, 1, dto)).ReturnsAsync(response);

            // Act
            var result = await _controller.CheckIn(1, dto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task CheckIn_ShouldReturnUnauthorized_WhenStudentIdMissing()
        {
            // Arrange
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var result = await _controller.CheckIn(1, new CheckInDto()) as UnauthorizedObjectResult;

            // Assert
            result.Should().NotBeNull();
        }
    }
}
