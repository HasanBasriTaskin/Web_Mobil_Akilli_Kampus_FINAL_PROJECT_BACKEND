using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Course;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class CoursesControllerTests
    {
        private readonly Mock<ICourseService> _mockService;
        private readonly CoursesController _controller;

        public CoursesControllerTests()
        {
            _mockService = new Mock<ICourseService>();
            _controller = new CoursesController(_mockService.Object);
        }

        [Fact]
        public async Task GetAllCourses_ShouldReturnStatusCode()
        {
            // Arrange
            var response = Response<IEnumerable<CourseListDto>>.Success(new List<CourseListDto>(), 200);
            _mockService.Setup(x => x.GetAllCoursesAsync(1, 10, null, null)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetAllCourses(1, 10, null, null) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetCourseById_ShouldReturnStatusCode()
        {
            // Arrange
            var response = Response<CourseDto>.Success(new CourseDto(), 200);
            _mockService.Setup(x => x.GetCourseByIdAsync(1)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetCourseById(1) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task CreateCourse_ShouldReturnStatusCode()
        {
            // Arrange
            var dto = new CreateCourseDto();
            var response = Response<CourseDto>.Success(new CourseDto(), 201);
            _mockService.Setup(x => x.CreateCourseAsync(dto)).ReturnsAsync(response);

            // Act
            var result = await _controller.CreateCourse(dto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task UpdateCourse_ShouldReturnStatusCode()
        {
            // Arrange
            var dto = new UpdateCourseDto();
            var response = Response<CourseDto>.Success(new CourseDto(), 200);
            _mockService.Setup(x => x.UpdateCourseAsync(1, dto)).ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateCourse(1, dto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task DeleteCourse_ShouldReturnStatusCode()
        {
            // Arrange
            var response = Response<NoDataDto>.Success(204);
            _mockService.Setup(x => x.DeleteCourseAsync(1)).ReturnsAsync(response);

            // Act
            var result = await _controller.DeleteCourse(1) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(204);
        }
    }
}
