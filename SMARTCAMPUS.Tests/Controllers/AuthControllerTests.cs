using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Auth;
using System.Security.Claims;
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

        private void SetupHttpContext(string userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            _authController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WhenSuccessful()
        {
            var loginDto = new LoginDto { Email = "test@test.com", Password = "Password" };
            var tokenDto = new TokenDto { AccessToken = "acc", RefreshToken = "ref" };
            var response = Response<TokenDto>.Success(tokenDto, 200);

            _mockAuthService.Setup(x => x.LoginAsync(loginDto)).ReturnsAsync(response);

            var result = await _authController.Login(loginDto);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task Login_ShouldReturnBadRequest_WhenFailed()
        {
            var loginDto = new LoginDto { Email = "test@test.com", Password = "Password" };
            var response = Response<TokenDto>.Fail("Error", 400);

            _mockAuthService.Setup(x => x.LoginAsync(loginDto)).ReturnsAsync(response);

            var result = await _authController.Login(loginDto);

            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(400);
            objectResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task Register_ShouldReturnCreated_WhenSuccessful()
        {
            var registerDto = new RegisterUserDto { Email = "new@test.com" };
            var tokenDto = new TokenDto { AccessToken = "acc", RefreshToken = "ref" };
            var response = Response<TokenDto>.Success(tokenDto, 201);

            _mockAuthService.Setup(x => x.RegisterAsync(registerDto)).ReturnsAsync(response);

            var result = await _authController.Register(registerDto);

            var createdResult = result.Should().BeOfType<ObjectResult>().Subject;
            createdResult.StatusCode.Should().Be(201);
            createdResult.Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public async Task ForgotPassword_ShouldReturnStatusCode()
        {
            var dto = new ForgotPasswordDto { Email = "a@b.com" };
            var response = Response<NoDataDto>.Success(200);

            _mockAuthService.Setup(x => x.ForgotPasswordAsync(dto)).ReturnsAsync(response);

            var result = await _authController.ForgotPassword(dto);

            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnOk_WhenSuccessful()
        {
            var dto = new ResetPasswordDto { Token = "t", NewPassword = "p", Email = "e" };
            var response = Response<NoDataDto>.Success(200);

            _mockAuthService.Setup(x => x.ResetPasswordAsync(dto)).ReturnsAsync(response);

            var result = await _authController.ResetPassword(dto);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateTokenByRefreshToken_ShouldReturnOk()
        {
            var dto = new RefreshTokenDto { Token = "ref" };
            var response = Response<TokenDto>.Success(new TokenDto(), 200);

            _mockAuthService.Setup(x => x.CreateTokenByRefreshTokenAsync(dto.Token)).ReturnsAsync(response);

            var result = await _authController.CreateTokenByRefreshToken(dto);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task RevokeRefreshToken_ShouldReturnOk()
        {
            var dto = new RefreshTokenDto { Token = "ref" };
            var response = Response<NoDataDto>.Success(204);

            _mockAuthService.Setup(x => x.RevokeRefreshTokenAsync(dto.Token)).ReturnsAsync(response);

            var result = await _authController.RevokeRefreshToken(dto);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task VerifyEmail_ShouldReturnOk()
        {
            var response = Response<NoDataDto>.Success(200);
            _mockAuthService.Setup(x => x.VerifyEmailAsync("u1", "t1")).ReturnsAsync(response);

            var result = await _authController.VerifyEmail("u1", "t1");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Logout_ShouldReturnOk()
        {
            var dto = new RefreshTokenDto { Token = "ref" };
            var response = Response<NoDataDto>.Success(204);
            _mockAuthService.Setup(x => x.LogoutAsync(dto.Token)).ReturnsAsync(response);

            var result = await _authController.Logout(dto);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ChangePassword_ShouldReturnOk_WhenAuthorized()
        {
            var userId = "u1";
            SetupHttpContext(userId);
            var dto = new ChangePasswordDto { UserId = userId, OldPassword = "old", NewPassword = "new" };
            var response = Response<NoDataDto>.Success(204);
            _mockAuthService.Setup(x => x.ChangePasswordAsync(dto)).ReturnsAsync(response);

            var result = await _authController.ChangePassword(dto);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ChangePassword_ShouldReturnUnauthorized_WhenUserIdMismatch()
        {
            SetupHttpContext("u1");
            var dto = new ChangePasswordDto { UserId = "u2", OldPassword = "old", NewPassword = "new" };

            var result = await _authController.ChangePassword(dto);

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }
    }
}
