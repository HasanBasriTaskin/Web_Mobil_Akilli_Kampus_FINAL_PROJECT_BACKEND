using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Event;
using System.Security.Claims;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class EventCategoriesControllerTests
    {
        private readonly Mock<IEventCategoryService> _mockCategoryService;
        private readonly EventCategoriesController _controller;

        public EventCategoriesControllerTests()
        {
            _mockCategoryService = new Mock<IEventCategoryService>();
            _controller = new EventCategoriesController(_mockCategoryService.Object);
            SetupHttpContext("user1", "Admin");
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
        public async Task GetAll_ShouldReturnOk()
        {
            _mockCategoryService.Setup(x => x.GetAllAsync(false))
                .ReturnsAsync(Response<List<EventCategoryDto>>.Success(new List<EventCategoryDto>(), 200));

            var result = await _controller.GetAll();

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetById_ShouldReturnOk()
        {
            _mockCategoryService.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(Response<EventCategoryDto>.Success(new EventCategoryDto(), 200));

            var result = await _controller.GetById(1);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Create_ShouldReturnOk()
        {
            var dto = new EventCategoryCreateDto { Name = "Test", Description = "Desc" };
            _mockCategoryService.Setup(x => x.CreateAsync(dto))
                .ReturnsAsync(Response<EventCategoryDto>.Success(new EventCategoryDto(), 201));

            var result = await _controller.Create(dto);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task Update_ShouldReturnOk()
        {
            var dto = new EventCategoryUpdateDto { Name = "Updated" };
            _mockCategoryService.Setup(x => x.UpdateAsync(1, dto))
                .ReturnsAsync(Response<EventCategoryDto>.Success(new EventCategoryDto(), 200));

            var result = await _controller.Update(1, dto);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Delete_ShouldReturnOk()
        {
            _mockCategoryService.Setup(x => x.DeleteAsync(1))
                .ReturnsAsync(Response<NoDataDto>.Success(200));

            var result = await _controller.Delete(1);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }
    }
}

