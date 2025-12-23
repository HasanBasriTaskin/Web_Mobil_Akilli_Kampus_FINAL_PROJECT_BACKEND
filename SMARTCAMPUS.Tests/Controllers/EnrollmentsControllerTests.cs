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
using SMARTCAMPUS.EntityLayer.DTOs.Enrollment;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class EnrollmentsControllerTests
    {
        private readonly Mock<IEnrollmentService> _mockService;
        private readonly EnrollmentsController _controller;

        public EnrollmentsControllerTests()
        {
            _mockService = new Mock<IEnrollmentService>();
            _controller = new EnrollmentsController(_mockService.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("StudentId", "1"),
                new Claim("FacultyId", "2")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task EnrollInCourse_ShouldReturnStatusCode()
        {
            // Arrange
            var dto = new CreateEnrollmentDto();
            var response = Response<EnrollmentDto>.Success(new EnrollmentDto(), 201);
            _mockService.Setup(x => x.EnrollInCourseAsync(1, dto)).ReturnsAsync(response);

            // Act
            var result = await _controller.EnrollInCourse(dto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task DropCourse_ShouldReturnStatusCode()
        {
            // Arrange
            var response = Response<NoDataDto>.Success(200);
            _mockService.Setup(x => x.DropCourseAsync(1, 1)).ReturnsAsync(response);

            // Act
            var result = await _controller.DropCourse(1) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetMyCourses_ShouldReturnStatusCode()
        {
            // Arrange
            var response = Response<IEnumerable<StudentCourseDto>>.Success(new List<StudentCourseDto>(), 200);
            _mockService.Setup(x => x.GetMyCoursesAsync(1)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetMyCourses() as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task ApproveEnrollment_ShouldReturnStatusCode()
        {
            // Arrange
            var response = Response<NoDataDto>.Success(200);
            _mockService.Setup(x => x.ApproveEnrollmentAsync(1, 2)).ReturnsAsync(response);

            // Act
            var result = await _controller.ApproveEnrollment(1) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetStudentsBySection_ShouldReturnStatusCode()
        {
            // Arrange
            var response = Response<IEnumerable<SectionStudentDto>>.Success(new List<SectionStudentDto>(), 200);
            _mockService.Setup(x => x.GetStudentsBySectionAsync(1, 2)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetStudentsBySection(1) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task CheckPrerequisites_ShouldReturnStatusCode()
        {
            // Arrange
            var response = Response<NoDataDto>.Success(200);
            _mockService.Setup(x => x.CheckPrerequisitesAsync(1, 1)).ReturnsAsync(response);

            // Act
            var result = await _controller.CheckPrerequisites(1) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task CheckScheduleConflict_ShouldReturnStatusCode()
        {
            // Arrange
            var response = Response<NoDataDto>.Success(200);
            _mockService.Setup(x => x.CheckScheduleConflictAsync(1, 1)).ReturnsAsync(response);

            // Act
            var result = await _controller.CheckScheduleConflict(1) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetMySections_ShouldReturnStatusCode()
        {
            // Arrange
            var response = Response<IEnumerable<FacultySectionDto>>.Success(new List<FacultySectionDto>(), 200);
            _mockService.Setup(x => x.GetMySectionsAsync(2)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetMySections() as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetPendingEnrollments_ShouldReturnStatusCode()
        {
            // Arrange
            var response = Response<IEnumerable<PendingEnrollmentDto>>.Success(new List<PendingEnrollmentDto>(), 200);
            _mockService.Setup(x => x.GetPendingEnrollmentsAsync(1, 2)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetPendingEnrollments(1) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task RejectEnrollment_ShouldReturnStatusCode()
        {
            // Arrange
            var response = Response<NoDataDto>.Success(200);
            _mockService.Setup(x => x.RejectEnrollmentAsync(1, 2, null)).ReturnsAsync(response);

            // Act
            var result = await _controller.RejectEnrollment(1, null) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task RejectEnrollment_WithReason_ShouldReturnStatusCode()
        {
            // Arrange
            var dto = new RejectEnrollmentDto { Reason = "Class is full" };
            var response = Response<NoDataDto>.Success(200);
            _mockService.Setup(x => x.RejectEnrollmentAsync(1, 2, "Class is full")).ReturnsAsync(response);

            // Act
            var result = await _controller.RejectEnrollment(1, dto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task EnrollInCourse_NoStudentId_ShouldReturnUnauthorized()
        {
            // Arrange - Create controller with no StudentId claim
            var userWithoutStudent = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("FacultyId", "2")
            }, "mock"));

            var controller = new EnrollmentsController(_mockService.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userWithoutStudent }
            };

            // Act
            var result = await controller.EnrollInCourse(new CreateEnrollmentDto()) as UnauthorizedObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(401);
        }
    }
}
