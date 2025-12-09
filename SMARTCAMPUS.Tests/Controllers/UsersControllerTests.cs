using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs.User;
using SMARTCAMPUS.EntityLayer.DTOs;
using System.Security.Claims;
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

        private void SetupHttpContext(string userId, string[] roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            _usersController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task GetMyProfile_ShouldReturnOk_WhenAuthenticated()
        {
            // Arrange
            var userId = "u1";
            SetupHttpContext(userId, Array.Empty<string>());
            var userDto = new UserDto { Id = userId, Email = "test@test.com", FullName = "Test", UserType = "Student", Role = "Student", Roles = new List<string> { "Student" } };
            var response = Response<UserDto>.Success(userDto, 200);

            _mockUserService.Setup(s => s.GetUserByIdAsync(userId))
                .ReturnsAsync(response);

            // Act
            var result = await _usersController.GetMyProfile();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task GetMyProfile_ShouldReturnUnauthorized_WhenUserNotIdentified()
        {
            // Arrange
            _usersController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext() // No User
            };

            // Act
            var result = await _usersController.GetMyProfile();

            // Assert
            result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task UpdateMyProfile_ShouldReturnOk_WhenValid()
        {
            // Arrange
            var userId = "u1";
            SetupHttpContext(userId, Array.Empty<string>());
            var dto = new UserUpdateDto { FullName = "NewName" };
            var response = Response<NoDataDto>.Success(204);

            _mockUserService.Setup(s => s.UpdateUserAsync(userId, dto))
                .ReturnsAsync(response);

            // Act
            var result = await _usersController.UpdateMyProfile(dto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task UploadProfilePicture_ShouldReturnOk_WhenValid()
        {
            // Arrange
            var userId = "u1";
            SetupHttpContext(userId, Array.Empty<string>());
            var fileMock = new Mock<IFormFile>();
            var response = Response<string>.Success("path/to/img.jpg", 200);

            _mockUserService.Setup(s => s.UploadProfilePictureAsync(userId, fileMock.Object))
                .ReturnsAsync(response);

            // Act
            var result = await _usersController.UploadProfilePicture(fileMock.Object);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task GetUsers_ShouldReturnOk_WhenAdmin()
        {
            // Arrange
            SetupHttpContext("admin1", new[] { "Admin" });
            var queryParams = new UserQueryParameters();
            var pagedData = new PagedResponse<UserListDto>(new List<UserListDto>(), 1, 10, 0);
            var response = Response<PagedResponse<UserListDto>>.Success(pagedData, 200);

            _mockUserService.Setup(s => s.GetUsersAsync(queryParams))
                .ReturnsAsync(response);

            // Act
            var result = await _usersController.GetUsers(queryParams);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnOk_WhenSelf()
        {
            // Arrange
            var userId = "u1";
            SetupHttpContext(userId, Array.Empty<string>());
            var userDto = new UserDto { Id = userId, Email = "test@test.com", FullName = "Test", UserType = "Student", Role = "Student", Roles = new List<string> { "Student" } };
            var response = Response<UserDto>.Success(userDto, 200);

            _mockUserService.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(response);

            // Act
            var result = await _usersController.GetUserById(userId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnForbidden_WhenNotSelfAndNotAdmin()
        {
            // Arrange
            var userId = "u1";
            SetupHttpContext(userId, Array.Empty<string>()); // Logged in as u1
            var otherUserId = "u2";

            // Act
            var result = await _usersController.GetUserById(otherUserId);

            // Assert
            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(403);
            statusResult.Value.Should().Be("Access Denied: You can only view your own profile.");
        }

        [Fact]
        public async Task GetUserById_ShouldReturnOk_WhenAdminAndNotSelf()
        {
            // Arrange
            SetupHttpContext("admin1", new[] { "Admin" });
            var otherUserId = "u2";
            var userDto = new UserDto { Id = otherUserId, Email = "other@test.com", FullName = "Other", UserType = "Student", Role = "Student", Roles = new List<string> { "Student" } };
            var response = Response<UserDto>.Success(userDto, 200);

            _mockUserService.Setup(s => s.GetUserByIdAsync(otherUserId)).ReturnsAsync(response);

            // Act
            var result = await _usersController.GetUserById(otherUserId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnOk_WhenSelf()
        {
            // Arrange
            var userId = "u1";
            SetupHttpContext(userId, Array.Empty<string>());
            var dto = new UserUpdateDto();
            var response = Response<NoDataDto>.Success(204);

            _mockUserService.Setup(s => s.UpdateUserAsync(userId, dto)).ReturnsAsync(response);

            // Act
            var result = await _usersController.UpdateUser(userId, dto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnForbidden_WhenNotSelfAndNotAdmin()
        {
            // Arrange
            SetupHttpContext("u1", Array.Empty<string>());
            var otherUserId = "u2";
            var dto = new UserUpdateDto();

            // Act
            var result = await _usersController.UpdateUser(otherUserId, dto);

            // Assert
            var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(403);
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnOk_WhenAdmin()
        {
            // Arrange
            SetupHttpContext("admin1", new[] { "Admin" });
            var userIdToDelete = "u2";
            var response = Response<NoDataDto>.Success(204);

            _mockUserService.Setup(s => s.DeleteUserAsync(userIdToDelete)).ReturnsAsync(response);

            // Act
            var result = await _usersController.DeleteUser(userIdToDelete);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AssignRoles_ShouldReturnOk_WhenAdmin()
        {
            // Arrange
            SetupHttpContext("admin1", new[] { "Admin" });
            var userId = "u2";
            var roles = new List<string> { "Manager" };
            var response = Response<NoDataDto>.Success(204);

            _mockUserService.Setup(s => s.AssignRolesAsync(userId, roles)).ReturnsAsync(response);

            // Act
            var result = await _usersController.AssignRoles(userId, roles);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
