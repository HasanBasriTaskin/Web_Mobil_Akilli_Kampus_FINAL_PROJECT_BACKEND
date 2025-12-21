using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class AcademicCalendarsControllerTests
    {
        private readonly Mock<IAcademicCalendarService> _mockService;
        private readonly AcademicCalendarsController _controller;

        public AcademicCalendarsControllerTests()
        {
            _mockService = new Mock<IAcademicCalendarService>();
            _controller = new AcademicCalendarsController(_mockService.Object);
        }

        #region GetCalendars Tests

        [Fact]
        public async Task GetCalendars_ShouldReturnStatusCode_WhenNoParameters()
        {
            // Arrange
            var calendars = new List<AcademicCalendarDto>
            {
                new AcademicCalendarDto { Id = 1, Title = "Spring Semester Start" }
            };
            var response = Response<IEnumerable<AcademicCalendarDto>>.Success(calendars, 200);
            _mockService.Setup(x => x.GetCalendarsAsync(null, null)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetCalendars(null, null) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetCalendars_ShouldReturnStatusCode_WithYearParameter()
        {
            // Arrange
            var calendars = new List<AcademicCalendarDto>();
            var response = Response<IEnumerable<AcademicCalendarDto>>.Success(calendars, 200);
            _mockService.Setup(x => x.GetCalendarsAsync(2024, null)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetCalendars(2024, null) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetCalendars_ShouldReturnStatusCode_WithSemesterParameter()
        {
            // Arrange
            var calendars = new List<AcademicCalendarDto>();
            var response = Response<IEnumerable<AcademicCalendarDto>>.Success(calendars, 200);
            _mockService.Setup(x => x.GetCalendarsAsync(null, "Fall")).ReturnsAsync(response);

            // Act
            var result = await _controller.GetCalendars(null, "Fall") as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetCalendars_ShouldReturnStatusCode_WithAllParameters()
        {
            // Arrange
            var calendars = new List<AcademicCalendarDto>();
            var response = Response<IEnumerable<AcademicCalendarDto>>.Success(calendars, 200);
            _mockService.Setup(x => x.GetCalendarsAsync(2024, "Spring")).ReturnsAsync(response);

            // Act
            var result = await _controller.GetCalendars(2024, "Spring") as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
            _mockService.Verify(x => x.GetCalendarsAsync(2024, "Spring"), Times.Once);
        }

        #endregion

        #region GetCalendarsByDateRange Tests

        [Fact]
        public async Task GetCalendarsByDateRange_ShouldReturnStatusCode()
        {
            // Arrange
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 12, 31);
            var calendars = new List<AcademicCalendarDto>();
            var response = Response<IEnumerable<AcademicCalendarDto>>.Success(calendars, 200);
            _mockService.Setup(x => x.GetCalendarsByDateRangeAsync(startDate, endDate)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetCalendarsByDateRange(startDate, endDate) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
            _mockService.Verify(x => x.GetCalendarsByDateRangeAsync(startDate, endDate), Times.Once);
        }

        #endregion

        #region GetUpcomingEvents Tests

        [Fact]
        public async Task GetUpcomingEvents_ShouldReturnStatusCode_WithDefaultDays()
        {
            // Arrange
            var calendars = new List<AcademicCalendarDto>();
            var response = Response<IEnumerable<AcademicCalendarDto>>.Success(calendars, 200);
            _mockService.Setup(x => x.GetUpcomingEventsAsync(30)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetUpcomingEvents() as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
            _mockService.Verify(x => x.GetUpcomingEventsAsync(30), Times.Once);
        }

        [Fact]
        public async Task GetUpcomingEvents_ShouldReturnStatusCode_WithCustomDays()
        {
            // Arrange
            var calendars = new List<AcademicCalendarDto>();
            var response = Response<IEnumerable<AcademicCalendarDto>>.Success(calendars, 200);
            _mockService.Setup(x => x.GetUpcomingEventsAsync(7)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetUpcomingEvents(7) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
            _mockService.Verify(x => x.GetUpcomingEventsAsync(7), Times.Once);
        }

        #endregion
    }
}
