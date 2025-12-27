using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.FoodItem;
using SMARTCAMPUS.EntityLayer.Enums;
using System.Security.Claims;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class FoodItemsControllerTests
    {
        private readonly Mock<IFoodItemService> _mockFoodItemService;
        private readonly FoodItemsController _controller;

        public FoodItemsControllerTests()
        {
            _mockFoodItemService = new Mock<IFoodItemService>();
            _controller = new FoodItemsController(_mockFoodItemService.Object);
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
            _mockFoodItemService.Setup(x => x.GetAllAsync(false))
                .ReturnsAsync(Response<List<FoodItemDto>>.Success(new List<FoodItemDto>(), 200));

            var result = await _controller.GetAll();

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetByCategory_ShouldReturnOk()
        {
            _mockFoodItemService.Setup(x => x.GetByCategoryAsync(MealItemCategory.MainCourse))
                .ReturnsAsync(Response<List<FoodItemDto>>.Success(new List<FoodItemDto>(), 200));

            var result = await _controller.GetByCategory(MealItemCategory.MainCourse);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetById_ShouldReturnOk()
        {
            _mockFoodItemService.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(Response<FoodItemDto>.Success(new FoodItemDto(), 200));

            var result = await _controller.GetById(1);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Create_ShouldReturnOk()
        {
            var dto = new FoodItemCreateDto { Name = "Test", Category = MealItemCategory.MainCourse };
            _mockFoodItemService.Setup(x => x.CreateAsync(dto))
                .ReturnsAsync(Response<FoodItemDto>.Success(new FoodItemDto(), 201));

            var result = await _controller.Create(dto);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task Update_ShouldReturnOk()
        {
            var dto = new FoodItemUpdateDto { Name = "Updated" };
            _mockFoodItemService.Setup(x => x.UpdateAsync(1, dto))
                .ReturnsAsync(Response<FoodItemDto>.Success(new FoodItemDto(), 200));

            var result = await _controller.Update(1, dto);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Delete_ShouldReturnOk()
        {
            _mockFoodItemService.Setup(x => x.DeleteAsync(1))
                .ReturnsAsync(Response<NoDataDto>.Success(200));

            var result = await _controller.Delete(1);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }
    }
}

