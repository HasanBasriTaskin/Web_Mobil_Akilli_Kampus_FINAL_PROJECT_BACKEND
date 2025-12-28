using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Menu;
using SMARTCAMPUS.EntityLayer.Enums;
using System.Security.Claims;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class MealMenusControllerTests
    {
        private readonly Mock<IMealMenuService> _mockMenuService;
        private readonly MealMenusController _controller;

        public MealMenusControllerTests()
        {
            _mockMenuService = new Mock<IMealMenuService>();
            _controller = new MealMenusController(_mockMenuService.Object);
        }

        private void SetupHttpContext(string? userId = null, string? role = null)
        {
            var claims = new List<Claim>();
            if (!string.IsNullOrEmpty(userId))
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            if (!string.IsNullOrEmpty(role))
                claims.Add(new Claim(ClaimTypes.Role, role));
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task GetMenus_ShouldReturnOk()
        {
            _mockMenuService.Setup(x => x.GetMenusAsync(null, null, null))
                .ReturnsAsync(Response<List<MealMenuListDto>>.Success(new List<MealMenuListDto>(), 200));

            var result = await _controller.GetMenus(null, null, null);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetById_ShouldReturnOk()
        {
            _mockMenuService.Setup(x => x.GetMenuByIdAsync(1))
                .ReturnsAsync(Response<MealMenuDto>.Success(new MealMenuDto(), 200));

            var result = await _controller.GetById(1);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Create_ShouldReturnOk()
        {
            SetupHttpContext("user1", "Admin");
            var dto = new MealMenuCreateDto { CafeteriaId = 1, Date = DateTime.Today, MealType = MealType.Lunch };
            _mockMenuService.Setup(x => x.CreateMenuAsync(dto))
                .ReturnsAsync(Response<MealMenuDto>.Success(new MealMenuDto(), 201));

            var result = await _controller.Create(dto);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task Update_ShouldReturnOk()
        {
            SetupHttpContext("user1", "Admin");
            var dto = new MealMenuCreateDto { CafeteriaId = 1, Date = DateTime.Today, MealType = MealType.Lunch };
            _mockMenuService.Setup(x => x.UpdateMenuAsync(1, dto))
                .ReturnsAsync(Response<MealMenuDto>.Success(new MealMenuDto(), 200));

            var result = await _controller.Update(1, dto);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Delete_ShouldReturnOk()
        {
            SetupHttpContext("user1", "Admin");
            _mockMenuService.Setup(x => x.DeleteMenuAsync(1, false))
                .ReturnsAsync(Response<NoDataDto>.Success(200));

            var result = await _controller.Delete(1);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Publish_ShouldReturnOk()
        {
            SetupHttpContext("user1", "Admin");
            _mockMenuService.Setup(x => x.PublishMenuAsync(1))
                .ReturnsAsync(Response<NoDataDto>.Success(200));

            var result = await _controller.Publish(1);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Unpublish_ShouldReturnOk()
        {
            SetupHttpContext("user1", "Admin");
            _mockMenuService.Setup(x => x.UnpublishMenuAsync(1))
                .ReturnsAsync(Response<NoDataDto>.Success(200));

            var result = await _controller.Unpublish(1);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task AddFoodItem_ShouldReturnOk()
        {
            SetupHttpContext("user1", "Admin");
            _mockMenuService.Setup(x => x.AddFoodItemToMenuAsync(1, 1))
                .ReturnsAsync(Response<NoDataDto>.Success(200));

            var result = await _controller.AddFoodItem(1, 1);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task RemoveFoodItem_ShouldReturnOk()
        {
            SetupHttpContext("user1", "Admin");
            _mockMenuService.Setup(x => x.RemoveFoodItemFromMenuAsync(1, 1))
                .ReturnsAsync(Response<NoDataDto>.Success(200));

            var result = await _controller.RemoveFoodItem(1, 1);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }
    }
}

