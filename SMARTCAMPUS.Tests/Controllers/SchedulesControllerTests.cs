using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs.Scheduling;
using System.Security.Claims;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class SchedulesControllerTests
    {
        private readonly Mock<IScheduleService> _mockScheduleService;
        private readonly SchedulesController _controller;

        public SchedulesControllerTests()
        {
            _mockScheduleService = new Mock<IScheduleService>();
            _controller = new SchedulesController(_mockScheduleService.Object);
        }

        [Fact]
        public async Task GetBySection_ShouldReturnOk()
        {
            _mockScheduleService.Setup(x => x.GetSchedulesBySectionAsync(1))
                .ReturnsAsync(Response<List<ScheduleDto>>.Success(new List<ScheduleDto>(), 200));

            var result = await _controller.GetBySection(1);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetWeeklySchedule_ShouldReturnOk()
        {
            _mockScheduleService.Setup(x => x.GetWeeklyScheduleAsync(1))
                .ReturnsAsync(Response<List<WeeklyScheduleDto>>.Success(new List<WeeklyScheduleDto>(), 200));

            var result = await _controller.GetWeeklySchedule(1);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetByClassroom_ShouldReturnOk()
        {
            _mockScheduleService.Setup(x => x.GetSchedulesByClassroomAsync(1, null))
                .ReturnsAsync(Response<List<ScheduleDto>>.Success(new List<ScheduleDto>(), 200));

            var result = await _controller.GetByClassroom(1, null);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetByInstructor_ShouldReturnOk()
        {
            _mockScheduleService.Setup(x => x.GetSchedulesByInstructorAsync(1, null))
                .ReturnsAsync(Response<List<ScheduleDto>>.Success(new List<ScheduleDto>(), 200));

            var result = await _controller.GetByInstructor(1, null);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Create_ShouldReturnCreated()
        {
            var dto = new ScheduleCreateDto { SectionId = 1, ClassroomId = 1 };
            _mockScheduleService.Setup(x => x.CreateScheduleAsync(dto))
                .ReturnsAsync(Response<ScheduleDto>.Success(new ScheduleDto(), 201));

            var result = await _controller.Create(dto);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task Update_ShouldReturnOk()
        {
            var dto = new ScheduleUpdateDto { DayOfWeek = System.DayOfWeek.Monday };
            _mockScheduleService.Setup(x => x.UpdateScheduleAsync(1, dto))
                .ReturnsAsync(Response<ScheduleDto>.Success(new ScheduleDto(), 200));

            var result = await _controller.Update(1, dto);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Delete_ShouldReturnOk()
        {
            _mockScheduleService.Setup(x => x.DeleteScheduleAsync(1))
                .ReturnsAsync(Response<SMARTCAMPUS.EntityLayer.DTOs.NoDataDto>.Success(200));

            var result = await _controller.Delete(1);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task CheckConflicts_ShouldReturnOk()
        {
            var dto = new ScheduleCreateDto { SectionId = 1, ClassroomId = 1 };
            _mockScheduleService.Setup(x => x.CheckConflictsAsync(dto, null))
                .ReturnsAsync(Response<List<ScheduleConflictDto>>.Success(new List<ScheduleConflictDto>(), 200));

            var result = await _controller.CheckConflicts(dto, null);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GenerateAutomaticSchedule_ShouldReturnOk()
        {
            var dto = new AutoScheduleRequestDto { Semester = "Fall", Year = 2024 };
            _mockScheduleService.Setup(x => x.GenerateAutomaticScheduleAsync(dto))
                .ReturnsAsync(Response<AutoScheduleResultDto>.Success(new AutoScheduleResultDto(), 200));

            var result = await _controller.GenerateAutomaticSchedule(dto);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task ExportSectionToICal_ShouldReturnFile_WhenSuccessful()
        {
            var icalContent = "BEGIN:VCALENDAR\nEND:VCALENDAR";
            _mockScheduleService.Setup(x => x.ExportSectionToICalAsync(1))
                .ReturnsAsync(Response<string>.Success(icalContent, 200));

            var result = await _controller.ExportSectionToICal(1);

            result.Should().BeOfType<FileContentResult>();
            var fileResult = (FileContentResult)result;
            fileResult.ContentType.Should().Be("text/calendar");
            fileResult.FileDownloadName.Should().Be("section_1_schedule.ics");
        }

        [Fact]
        public async Task ExportSectionToICal_ShouldReturnError_WhenFailed()
        {
            _mockScheduleService.Setup(x => x.ExportSectionToICalAsync(1))
                .ReturnsAsync(Response<string>.Fail("Error", 404));

            var result = await _controller.ExportSectionToICal(1);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task ExportMyScheduleToICal_ShouldReturnFile_WhenSuccessful()
        {
            SetupHttpContext("user1");
            var icalContent = "BEGIN:VCALENDAR\nEND:VCALENDAR";
            _mockScheduleService.Setup(x => x.ExportStudentScheduleToICalAsync("user1"))
                .ReturnsAsync(Response<string>.Success(icalContent, 200));

            var result = await _controller.ExportMyScheduleToICal();

            result.Should().BeOfType<FileContentResult>();
            var fileResult = (FileContentResult)result;
            fileResult.ContentType.Should().Be("text/calendar");
            fileResult.FileDownloadName.Should().Be("my_schedule.ics");
        }

        [Fact]
        public async Task ExportMyScheduleToICal_ShouldReturnUnauthorized_WhenUserIdIsNull()
        {
            SetupHttpContext(null);

            var result = await _controller.ExportMyScheduleToICal();

            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task ExportMyScheduleToICal_ShouldReturnError_WhenFailed()
        {
            SetupHttpContext("user1");
            _mockScheduleService.Setup(x => x.ExportStudentScheduleToICalAsync("user1"))
                .ReturnsAsync(Response<string>.Fail("Error", 404));

            var result = await _controller.ExportMyScheduleToICal();

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(404);
        }

        private void SetupHttpContext(string? userId)
        {
            var claims = new List<Claim>();
            if (!string.IsNullOrEmpty(userId))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            }
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }
    }
}

