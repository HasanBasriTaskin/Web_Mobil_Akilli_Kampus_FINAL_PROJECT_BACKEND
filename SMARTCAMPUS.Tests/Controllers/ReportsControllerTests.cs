using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using System.Security.Claims;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class ReportsControllerTests
    {
        private readonly Mock<IReportExportService> _mockReportService;
        private readonly ReportsController _controller;

        public ReportsControllerTests()
        {
            _mockReportService = new Mock<IReportExportService>();
            _controller = new ReportsController(_mockReportService.Object);
            SetupHttpContext("user1");
        }

        private void SetupHttpContext(string userId)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task ExportStudentListToExcel_ShouldReturnFile()
        {
            var bytes = new byte[] { 1, 2, 3 };
            _mockReportService.Setup(x => x.ExportStudentListToExcelAsync(null))
                .ReturnsAsync(bytes);

            var result = await _controller.ExportStudentListToExcel();

            result.Should().BeOfType<FileContentResult>();
            ((FileContentResult)result).ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        [Fact]
        public async Task ExportGradeReportToExcel_ShouldReturnFile()
        {
            var bytes = new byte[] { 1, 2, 3 };
            _mockReportService.Setup(x => x.ExportGradeReportToExcelAsync(1))
                .ReturnsAsync(bytes);

            var result = await _controller.ExportGradeReportToExcel(1);

            result.Should().BeOfType<FileContentResult>();
            ((FileContentResult)result).ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        [Fact]
        public async Task ExportGradeReportToExcel_ShouldReturnNotFound_WhenArgumentException()
        {
            _mockReportService.Setup(x => x.ExportGradeReportToExcelAsync(1))
                .ThrowsAsync(new ArgumentException("Section not found"));

            var result = await _controller.ExportGradeReportToExcel(1);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task ExportTranscriptToPdf_ShouldReturnFile()
        {
            var bytes = new byte[] { 1, 2, 3 };
            _mockReportService.Setup(x => x.ExportTranscriptToPdfAsync(1))
                .ReturnsAsync(bytes);

            var result = await _controller.ExportTranscriptToPdf(1);

            result.Should().BeOfType<FileContentResult>();
            ((FileContentResult)result).ContentType.Should().Be("application/pdf");
        }

        [Fact]
        public async Task ExportAttendanceReportToPdf_ShouldReturnFile()
        {
            var bytes = new byte[] { 1, 2, 3 };
            _mockReportService.Setup(x => x.ExportAttendanceReportToPdfAsync(1))
                .ReturnsAsync(bytes);

            var result = await _controller.ExportAttendanceReportToPdf(1);

            result.Should().BeOfType<FileContentResult>();
            ((FileContentResult)result).ContentType.Should().Be("application/pdf");
        }

        [Fact]
        public async Task ExportAtRiskStudentsToExcel_ShouldReturnFile()
        {
            var bytes = new byte[] { 1, 2, 3 };
            _mockReportService.Setup(x => x.ExportAtRiskStudentsToExcelAsync(2.0))
                .ReturnsAsync(bytes);

            var result = await _controller.ExportAtRiskStudentsToExcel(2.0);

            result.Should().BeOfType<FileContentResult>();
            ((FileContentResult)result).ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
    }
}

