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

        private void SetupFacultyUser()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("FacultyId", "2"),
                new Claim(ClaimTypes.Role, "Faculty")
            }, "mock"));
            _controller.ControllerContext.HttpContext.User = user;
        }

        private void SetupStudentUser()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("StudentId", "1"),
                new Claim(ClaimTypes.Role, "Student")
            }, "mock"));
            _controller.ControllerContext.HttpContext.User = user;
        }

        private void SetupEmptyUser()
        {
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        }

        #region Session Management Tests

        [Fact]
        public async Task CreateSession_ShouldReturnStatusCode()
        {
            // Arrange
            SetupFacultyUser();
            var dto = new CreateSessionDto();
            var response = Response<AttendanceSessionDto>.Success(new AttendanceSessionDto(), 201);
            _mockService.Setup(x => x.CreateSessionAsync(2, dto)).ReturnsAsync(response);

            // Act
            var result = await _controller.CreateSession(dto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task CreateSession_ShouldReturnUnauthorized_WhenFacultyIdMissing()
        {
            // Arrange
            SetupEmptyUser();

            // Act
            var result = await _controller.CreateSession(new CreateSessionDto()) as UnauthorizedObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().Be("Faculty ID not found");
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
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task CloseSession_ShouldReturnStatusCode_WhenSuccess()
        {
            // Arrange
            SetupFacultyUser();
            var response = Response<NoDataDto>.Success(200);
            _mockService.Setup(x => x.CloseSessionAsync(2, 1)).ReturnsAsync(response);

            // Act
            var result = await _controller.CloseSession(1) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task CloseSession_ShouldReturnUnauthorized_WhenFacultyIdMissing()
        {
            // Arrange
            SetupEmptyUser();

            // Act
            var result = await _controller.CloseSession(1) as UnauthorizedObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().Be("Faculty ID not found");
        }

        [Fact]
        public async Task GetMySessions_ShouldReturnStatusCode_WhenSuccess()
        {
            // Arrange
            SetupFacultyUser();
            var sessions = new List<AttendanceSessionDto>();
            var response = Response<IEnumerable<AttendanceSessionDto>>.Success(sessions, 200);
            _mockService.Setup(x => x.GetMySessionsAsync(2)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetMySessions() as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetMySessions_ShouldReturnUnauthorized_WhenFacultyIdMissing()
        {
            // Arrange
            SetupEmptyUser();

            // Act
            var result = await _controller.GetMySessions() as UnauthorizedObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().Be("Faculty ID not found");
        }

        [Fact]
        public async Task GetSessionRecords_ShouldReturnStatusCode()
        {
            // Arrange
            SetupFacultyUser();
            var records = new List<AttendanceRecordDto>();
            var response = Response<IEnumerable<AttendanceRecordDto>>.Success(records, 200);
            _mockService.Setup(x => x.GetSessionRecordsAsync(1)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetSessionRecords(1) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        #endregion

        #region Student Check-in Tests

        [Fact]
        public async Task CheckIn_ShouldReturnStatusCode()
        {
            // Arrange
            SetupStudentUser();
            var dto = new CheckInDto();
            var response = Response<CheckInResultDto>.Success(new CheckInResultDto(), 200);
            _mockService.Setup(x => x.CheckInAsync(1, 1, dto)).ReturnsAsync(response);

            // Act
            var result = await _controller.CheckIn(1, dto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task CheckIn_ShouldReturnUnauthorized_WhenStudentIdMissing()
        {
            // Arrange
            SetupEmptyUser();

            // Act
            var result = await _controller.CheckIn(1, new CheckInDto()) as UnauthorizedObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().Be("Student ID not found");
        }

        [Fact]
        public async Task GetMyAttendance_ShouldReturnStatusCode_WhenSuccess()
        {
            // Arrange
            SetupStudentUser();
            var attendance = new List<StudentAttendanceDto>();
            var response = Response<IEnumerable<StudentAttendanceDto>>.Success(attendance, 200);
            _mockService.Setup(x => x.GetMyAttendanceAsync(1)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetMyAttendance() as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetMyAttendance_ShouldReturnUnauthorized_WhenStudentIdMissing()
        {
            // Arrange
            SetupEmptyUser();

            // Act
            var result = await _controller.GetMyAttendance() as UnauthorizedObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().Be("Student ID not found");
        }

        #endregion

        #region Excuse Request Tests

        [Fact]
        public async Task CreateExcuseRequest_ShouldReturnStatusCode_WhenSuccess()
        {
            // Arrange
            SetupStudentUser();
            var dto = new CreateExcuseRequestDto { Reason = "Medical" };
            var responseDto = new ExcuseRequestDto { Id = 1, Reason = "Medical" };
            var response = Response<ExcuseRequestDto>.Success(responseDto, 201);
            _mockService.Setup(x => x.CreateExcuseRequestAsync(1, dto, null)).ReturnsAsync(response);

            // Act
            var result = await _controller.CreateExcuseRequest(dto, null) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task CreateExcuseRequest_ShouldReturnUnauthorized_WhenStudentIdMissing()
        {
            // Arrange
            SetupEmptyUser();

            // Act
            var result = await _controller.CreateExcuseRequest(new CreateExcuseRequestDto(), null) as UnauthorizedObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().Be("Student ID not found");
        }

        [Fact]
        public async Task GetExcuseRequests_ShouldReturnStatusCode_WhenSuccess()
        {
            // Arrange
            SetupFacultyUser();
            var requests = new List<ExcuseRequestDto>();
            var response = Response<IEnumerable<ExcuseRequestDto>>.Success(requests, 200);
            _mockService.Setup(x => x.GetExcuseRequestsAsync(2, null)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetExcuseRequests(null) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetExcuseRequests_ShouldReturnStatusCode_WithSectionId()
        {
            // Arrange
            SetupFacultyUser();
            var requests = new List<ExcuseRequestDto>();
            var response = Response<IEnumerable<ExcuseRequestDto>>.Success(requests, 200);
            _mockService.Setup(x => x.GetExcuseRequestsAsync(2, 5)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetExcuseRequests(5) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetExcuseRequests_ShouldReturnUnauthorized_WhenFacultyIdMissing()
        {
            // Arrange
            SetupEmptyUser();

            // Act
            var result = await _controller.GetExcuseRequests(null) as UnauthorizedObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().Be("Faculty ID not found");
        }

        [Fact]
        public async Task ApproveExcuseRequest_ShouldReturnStatusCode_WhenSuccess()
        {
            // Arrange
            SetupFacultyUser();
            var dto = new ReviewExcuseRequestDto { Notes = "Approved" };
            var response = Response<NoDataDto>.Success(200);
            _mockService.Setup(x => x.ApproveExcuseRequestAsync(2, 1, dto)).ReturnsAsync(response);

            // Act
            var result = await _controller.ApproveExcuseRequest(1, dto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task ApproveExcuseRequest_ShouldReturnUnauthorized_WhenFacultyIdMissing()
        {
            // Arrange
            SetupEmptyUser();

            // Act
            var result = await _controller.ApproveExcuseRequest(1, new ReviewExcuseRequestDto()) as UnauthorizedObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().Be("Faculty ID not found");
        }

        [Fact]
        public async Task RejectExcuseRequest_ShouldReturnStatusCode_WhenSuccess()
        {
            // Arrange
            SetupFacultyUser();
            var dto = new ReviewExcuseRequestDto { Notes = "Rejected" };
            var response = Response<NoDataDto>.Success(200);
            _mockService.Setup(x => x.RejectExcuseRequestAsync(2, 1, dto)).ReturnsAsync(response);

            // Act
            var result = await _controller.RejectExcuseRequest(1, dto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task RejectExcuseRequest_ShouldReturnUnauthorized_WhenFacultyIdMissing()
        {
            // Arrange
            SetupEmptyUser();

            // Act
            var result = await _controller.RejectExcuseRequest(1, new ReviewExcuseRequestDto()) as UnauthorizedObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().Be("Faculty ID not found");
        }

        #endregion

        #region Private Method Coverage Tests

        [Fact]
        public async Task CreateSession_ShouldParseFacultyIdCorrectly_WithValidClaim()
        {
            // Arrange - test with different FacultyId value
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("FacultyId", "100"),
                new Claim(ClaimTypes.Role, "Faculty")
            }, "mock"));
            _controller.ControllerContext.HttpContext.User = user;

            var dto = new CreateSessionDto();
            var response = Response<AttendanceSessionDto>.Success(new AttendanceSessionDto(), 201);
            _mockService.Setup(x => x.CreateSessionAsync(100, dto)).ReturnsAsync(response);

            // Act
            var result = await _controller.CreateSession(dto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(201);
            _mockService.Verify(x => x.CreateSessionAsync(100, dto), Times.Once);
        }

        [Fact]
        public async Task CheckIn_ShouldParseStudentIdCorrectly_WithValidClaim()
        {
            // Arrange - test with different StudentId value
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("StudentId", "50"),
                new Claim(ClaimTypes.Role, "Student")
            }, "mock"));
            _controller.ControllerContext.HttpContext.User = user;

            var dto = new CheckInDto();
            var response = Response<CheckInResultDto>.Success(new CheckInResultDto(), 200);
            _mockService.Setup(x => x.CheckInAsync(50, 1, dto)).ReturnsAsync(response);

            // Act
            var result = await _controller.CheckIn(1, dto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
            _mockService.Verify(x => x.CheckInAsync(50, 1, dto), Times.Once);
        }

        [Fact]
        public async Task CreateSession_ShouldReturnUnauthorized_WhenFacultyIdClaimHasInvalidValue()
        {
            // Arrange - non-parseable FacultyId
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("FacultyId", "not-a-number"),
                new Claim(ClaimTypes.Role, "Faculty")
            }, "mock"));
            _controller.ControllerContext.HttpContext.User = user;

            // Act
            var result = await _controller.CreateSession(new CreateSessionDto()) as UnauthorizedObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().Be("Faculty ID not found");
        }

        [Fact]
        public async Task CheckIn_ShouldReturnUnauthorized_WhenStudentIdClaimHasInvalidValue()
        {
            // Arrange - non-parseable StudentId
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("StudentId", "invalid"),
                new Claim(ClaimTypes.Role, "Student")
            }, "mock"));
            _controller.ControllerContext.HttpContext.User = user;

            // Act
            var result = await _controller.CheckIn(1, new CheckInDto()) as UnauthorizedObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().Be("Student ID not found");
        }

        #endregion
    }
}
