using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs.Auth;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _authController;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _authController = new AuthController(_mockAuthService.Object);
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "test@test.com", Password = "Password" };
            var tokenDto = new TokenDto { AccessToken = "acc", RefreshToken = "ref" };
            var response = Response<TokenDto>.Success(tokenDto, 200);

            _mockAuthService.Setup(x => x.LoginAsync(loginDto)).ReturnsAsync(response);

            // Act
            var result = await _authController.Login(loginDto);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task Login_ShouldReturnBadRequest_WhenFailed()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "test@test.com", Password = "Password" };
            var response = Response<TokenDto>.Fail("Error", 400);

            _mockAuthService.Setup(x => x.LoginAsync(loginDto)).ReturnsAsync(response);

            // Act
            var result = await _authController.Login(loginDto);

            // Assert
            // AuthController uses StatusCode(code, result) for failures, so it's ObjectResult
            var objectResult = result as ObjectResult;
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be(400);
            objectResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task Register_ShouldReturnCreated_WhenSuccessful()
        {
             // Arrange
            var registerDto = new RegisterUserDto { Email = "new@test.com" };
            var tokenDto = new TokenDto { AccessToken = "acc", RefreshToken = "ref" };
            var response = Response<TokenDto>.Success(tokenDto, 201);

            _mockAuthService.Setup(x => x.RegisterAsync(registerDto)).ReturnsAsync(response);

            // Act
            var result = await _authController.Register(registerDto);

            // Assert
            var createdResult = result as ObjectResult;
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be(201);
            createdResult.Value.Should().BeEquivalentTo(response);
        }
    }
}
