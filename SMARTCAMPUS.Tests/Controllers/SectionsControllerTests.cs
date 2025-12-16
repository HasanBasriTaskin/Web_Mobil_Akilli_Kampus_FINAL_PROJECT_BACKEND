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
    public class SectionsControllerTests
    {
        private readonly Mock<ICourseSectionService> _mockService;
        private readonly SectionsController _controller;

        public SectionsControllerTests()
        {
            _mockService = new Mock<ICourseSectionService>();
            _controller = new SectionsController(_mockService.Object);
        }

        [Fact]
        public async Task GetSections_ShouldReturnStatusCode()
        {
            // Arrange
            var query = new CourseSectionQueryParameters();
            var pagedData = new PagedResponse<CourseSectionDto>(new System.Collections.Generic.List<CourseSectionDto>(), 1, 10, 0);
            var response = Response<PagedResponse<CourseSectionDto>>.Success(pagedData, 200);
            _mockService.Setup(x => x.GetSectionsAsync(query)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetSections(query) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetSection_ShouldReturnStatusCode()
        {
            // Arrange
            var response = Response<CourseSectionDto>.Success(new CourseSectionDto(), 200);
            _mockService.Setup(x => x.GetSectionByIdAsync(1)).ReturnsAsync(response);

            // Act
            var result = await _controller.GetSection(1) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task CreateSection_ShouldReturnStatusCode()
        {
            // Arrange
            var dto = new CourseSectionCreateDto();
            var response = Response<CourseSectionDto>.Success(new CourseSectionDto(), 201);
            _mockService.Setup(x => x.CreateSectionAsync(dto)).ReturnsAsync(response);

            // Act
            var result = await _controller.CreateSection(dto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task UpdateSection_ShouldReturnStatusCode()
        {
            // Arrange
            var dto = new CourseSectionUpdateDto();
            var response = Response<CourseSectionDto>.Success(new CourseSectionDto(), 200);
            _mockService.Setup(x => x.UpdateSectionAsync(1, dto)).ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateSection(1, dto) as ObjectResult;

            // Assert
            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
        }
    }
}
