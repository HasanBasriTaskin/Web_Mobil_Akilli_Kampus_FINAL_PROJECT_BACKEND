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
    public class DepartmentsControllerTests
    {
        private readonly Mock<IDepartmentService> _mockService;
        private readonly DepartmentsController _controller;

        public DepartmentsControllerTests()
        {
            _mockService = new Mock<IDepartmentService>();
            _controller = new DepartmentsController(_mockService.Object);
        }

        [Fact]
        public async Task GetDepartments_ShouldReturnStatusCode_WhenSuccess()
        {
            // Arrange
            var departments = new List<DepartmentDto>
            {
                new DepartmentDto { Id = 1, Name = "Computer Science", Code = "CS" },
                new DepartmentDto { Id = 2, Name = "Electrical Engineering", Code = "EE" }
            };
            var response = Response<List<DepartmentDto>>.Success(departments, 200);
            _mockService.Setup(x => x.GetDepartmentsAsync()).ReturnsAsync(response);

            // Act
            var result = await _controller.GetDepartments() as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
            _mockService.Verify(x => x.GetDepartmentsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetDepartments_ShouldReturnEmptyList_WhenNoDepartments()
        {
            // Arrange
            var departments = new List<DepartmentDto>();
            var response = Response<List<DepartmentDto>>.Success(departments, 200);
            _mockService.Setup(x => x.GetDepartmentsAsync()).ReturnsAsync(response);

            // Act
            var result = await _controller.GetDepartments() as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetDepartments_ShouldReturnError_WhenServiceFails()
        {
            // Arrange
            var response = Response<List<DepartmentDto>>.Fail("Error retrieving departments", 500);
            _mockService.Setup(x => x.GetDepartmentsAsync()).ReturnsAsync(response);

            // Act
            var result = await _controller.GetDepartments() as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(500);
        }
    }
}
