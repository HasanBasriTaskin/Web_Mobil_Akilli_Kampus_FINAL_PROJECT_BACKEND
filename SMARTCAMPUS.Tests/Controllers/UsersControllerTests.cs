using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs.User;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UsersController _usersController;

        public UsersControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _usersController = new UsersController(_mockUserService.Object);
        }

        [Fact]
        public async Task GetUsers_ShouldReturnOk_WithPagedResponse()
        {
            // Arrange
            var queryParams = new UserQueryParameters();
            var pagedResponse = new PagedResponse<UserListDto>(new List<UserListDto>(), 1, 10, 0);

            _mockUserService.Setup(x => x.GetUsersAsync(queryParams)).ReturnsAsync(pagedResponse);

            // Act
            var result = await _usersController.GetUsers(queryParams);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(pagedResponse);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnOk_WhenFound()
        {
            // Arrange
            var userId = "1";
            var userDto = new UserProfileDto { IdString = "1", FullName = "Test" };
            var response = Response<UserProfileDto>.Success(userDto, 200);

            _mockUserService.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(response);

            // Mock User Claims
            var claims = new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1")
            };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "Test");
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);

            _usersController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _usersController.GetUserById(userId);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(response);
        }

         [Fact]
        public async Task GetUserById_ShouldReturnNotFound_WhenNotFound()
        {
            // Arrange
            var userId = "1";
            var response = Response<UserProfileDto>.Fail("Not found", 404);

            _mockUserService.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(response);

            // Mock User Claims
            var claims = new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1")
            };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "Test");
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);

            _usersController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _usersController.GetUserById(userId);

            // Assert
            // Controller returns StatusCode(404, result) not NotFoundObjectResult
            var objectResult = result as ObjectResult;
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be(404);
            objectResult.Value.Should().BeEquivalentTo(response);
        }
    }
}
