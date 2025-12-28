using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs.Analytics;
using System.Security.Claims;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class AnalyticsControllerTests
    {
        private readonly Mock<IAnalyticsService> _mockAnalyticsService;
        private readonly AnalyticsController _controller;

        public AnalyticsControllerTests()
        {
            _mockAnalyticsService = new Mock<IAnalyticsService>();
            _controller = new AnalyticsController(_mockAnalyticsService.Object);
            SetupHttpContext("admin1", "Admin");
        }

        private void SetupHttpContext(string userId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task GetDashboardStats_ShouldReturnOk()
        {
            _mockAnalyticsService.Setup(x => x.GetDashboardStatsAsync())
                .ReturnsAsync(Response<AdminDashboardDto>.Success(new AdminDashboardDto(), 200));

            var result = await _controller.GetDashboardStats();

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetAcademicPerformance_ShouldReturnOk()
        {
            _mockAnalyticsService.Setup(x => x.GetAcademicPerformanceAsync())
                .ReturnsAsync(Response<AcademicPerformanceDto>.Success(new AcademicPerformanceDto(), 200));

            var result = await _controller.GetAcademicPerformance();

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetDepartmentGpaStats_ShouldReturnOk()
        {
            _mockAnalyticsService.Setup(x => x.GetDepartmentGpaStatsAsync())
                .ReturnsAsync(Response<List<DepartmentGpaDto>>.Success(new List<DepartmentGpaDto>(), 200));

            var result = await _controller.GetDepartmentGpaStats();

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetDepartmentStats_ShouldReturnOk()
        {
            _mockAnalyticsService.Setup(x => x.GetDepartmentStatsAsync(1))
                .ReturnsAsync(Response<DepartmentGpaDto>.Success(new DepartmentGpaDto(), 200));

            var result = await _controller.GetDepartmentStats(1);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetGradeDistribution_ShouldReturnOk()
        {
            _mockAnalyticsService.Setup(x => x.GetGradeDistributionAsync(null))
                .ReturnsAsync(Response<List<GradeDistributionDto>>.Success(new List<GradeDistributionDto>(), 200));

            var result = await _controller.GetGradeDistribution();

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetGradeDistribution_ShouldReturnOk_WithSectionId()
        {
            _mockAnalyticsService.Setup(x => x.GetGradeDistributionAsync(1))
                .ReturnsAsync(Response<List<GradeDistributionDto>>.Success(new List<GradeDistributionDto>(), 200));

            var result = await _controller.GetGradeDistribution(1);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetAtRiskStudents_ShouldReturnOk()
        {
            _mockAnalyticsService.Setup(x => x.GetAtRiskStudentsAsync(2.0, 20.0))
                .ReturnsAsync(Response<List<AtRiskStudentDto>>.Success(new List<AtRiskStudentDto>(), 200));

            var result = await _controller.GetAtRiskStudents();

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetAtRiskStudents_ShouldReturnOk_WithCustomThresholds()
        {
            _mockAnalyticsService.Setup(x => x.GetAtRiskStudentsAsync(1.5, 30.0))
                .ReturnsAsync(Response<List<AtRiskStudentDto>>.Success(new List<AtRiskStudentDto>(), 200));

            var result = await _controller.GetAtRiskStudents(1.5, 30.0);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetCourseOccupancy_ShouldReturnOk()
        {
            _mockAnalyticsService.Setup(x => x.GetCourseOccupancyAsync())
                .ReturnsAsync(Response<List<CourseOccupancyDto>>.Success(new List<CourseOccupancyDto>(), 200));

            var result = await _controller.GetCourseOccupancy();

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetAttendanceStats_ShouldReturnOk()
        {
            _mockAnalyticsService.Setup(x => x.GetAttendanceStatsAsync())
                .ReturnsAsync(Response<AttendanceStatsDto>.Success(new AttendanceStatsDto(), 200));

            var result = await _controller.GetAttendanceStats();

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }
    }
}

