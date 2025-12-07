using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.BusinessLayer.Tools;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Auth;
using SMARTCAMPUS.EntityLayer.Models;
using SMARTCAMPUS.Tests.TestUtilities;
using System.Security.Claims;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class AuthManagerTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<SignInManager<User>> _mockSignInManager;
        private readonly Mock<JwtTokenGenerator> _mockTokenGenerator;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IRefreshTokenDal> _mockRefreshTokenDal;
        private readonly Mock<IStudentDal> _mockStudentDal;
        private readonly Mock<IEmailVerificationTokenDal> _mockEmailVerificationTokenDal;
        private readonly Mock<IPasswordResetTokenDal> _mockPasswordResetTokenDal;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IConfiguration> _mockConfiguration;

        private readonly AuthManager _authManager;

        public AuthManagerTests()
        {
            // Setup UserManager Mock
            var userStoreMock = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            // Setup SignInManager Mock
            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<User>>();
            _mockSignInManager = new Mock<SignInManager<User>>(
                _mockUserManager.Object, contextAccessorMock.Object, claimsFactoryMock.Object, null, null, null, null);

            // Setup other mocks
            _mockTokenGenerator = new Mock<JwtTokenGenerator>(new Mock<IConfiguration>().Object);
            _mockMapper = new Mock<IMapper>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockRefreshTokenDal = new Mock<IRefreshTokenDal>();
            _mockStudentDal = new Mock<IStudentDal>();
            _mockEmailVerificationTokenDal = new Mock<IEmailVerificationTokenDal>();
            _mockPasswordResetTokenDal = new Mock<IPasswordResetTokenDal>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockConfiguration = new Mock<IConfiguration>();

            // Basic UnitOfWork setup
            _mockUnitOfWork.Setup(u => u.RefreshTokens).Returns(_mockRefreshTokenDal.Object);
            _mockUnitOfWork.Setup(u => u.Students).Returns(_mockStudentDal.Object);
            _mockUnitOfWork.Setup(u => u.EmailVerificationTokens).Returns(_mockEmailVerificationTokenDal.Object);
            _mockUnitOfWork.Setup(u => u.PasswordResetTokens).Returns(_mockPasswordResetTokenDal.Object);

            _mockRefreshTokenDal.Setup(u => u.AddAsync(It.IsAny<RefreshToken>()))
                                .Returns(Task.CompletedTask);
            _mockStudentDal.Setup(u => u.AddAsync(It.IsAny<Student>()))
                           .Returns(Task.CompletedTask);
            _mockEmailVerificationTokenDal.Setup(u => u.AddAsync(It.IsAny<EmailVerificationToken>()))
                                          .Returns(Task.CompletedTask);
            _mockPasswordResetTokenDal.Setup(u => u.AddAsync(It.IsAny<PasswordResetToken>()))
                                      .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

            // Basic HttpContext setup
            var context = new DefaultHttpContext();
            _mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            _authManager = new AuthManager(
                _mockUserManager.Object,
                _mockSignInManager.Object,
                _mockTokenGenerator.Object,
                _mockMapper.Object,
                _mockUnitOfWork.Object,
                _mockHttpContextAccessor.Object,
                _mockNotificationService.Object,
                _mockConfiguration.Object
            );
        }

        #region LoginAsync Tests

        [Fact]
        public async Task LoginAsync_WithInvalidEmail_ReturnsFail()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "wrong@test.com", Password = "Password123" };
            _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
                            .ReturnsAsync((User?)null);

            // Act
            var result = await _authManager.LoginAsync(loginDto);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Errors.Should().Contain("Geçersiz e-posta veya şifre");
        }

        [Fact]
        public async Task LoginAsync_WithInactiveAccount_ReturnsFail()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "active@test.com", Password = "Password123" };
            var user = new User { Email = loginDto.Email, IsActive = false };

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
                            .ReturnsAsync(user);

            // Act
            var result = await _authManager.LoginAsync(loginDto);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Errors.Should().Contain("Hesap aktif değil. Lütfen e-postanızı doğrulayın.");
        }

        [Fact]
        public async Task LoginAsync_WithWrongPassword_ReturnsFail()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "user@test.com", Password = "WrongPassword" };
            var user = new User { Email = loginDto.Email, IsActive = true };

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
                            .ReturnsAsync(user);

            _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, loginDto.Password, false))
                              .ReturnsAsync(SignInResult.Failed);

            // Act
            var result = await _authManager.LoginAsync(loginDto);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Errors.Should().Contain("Geçersiz e-posta veya şifre");
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsSuccessAndToken()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "user@test.com", Password = "CorrectPassword" };
            var user = new User { Id = "u1", Email = loginDto.Email, IsActive = true };
            var roles = new List<string> { "Student" };
            var tokenDto = new TokenDto { AccessToken = "access_token", RefreshToken = "refresh_token" };

            _mockUserManager.Setup(x => x.FindByEmailAsync(loginDto.Email))
                            .ReturnsAsync(user);
            _mockSignInManager.Setup(x => x.CheckPasswordSignInAsync(user, loginDto.Password, false))
                              .ReturnsAsync(SignInResult.Success);
            _mockUserManager.Setup(x => x.GetRolesAsync(user))
                            .ReturnsAsync(roles);

            // Mock Token Generator
            _mockTokenGenerator.Setup(x => x.GenerateToken(user, roles))
                               .Returns(tokenDto);

            // Act
            var result = await _authManager.LoginAsync(loginDto);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(tokenDto);

            _mockRefreshTokenDal.Verify(x => x.AddAsync(It.Is<RefreshToken>(rt =>
                rt.UserId == user.Id &&
                rt.Token == tokenDto.RefreshToken)), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        }

        #endregion

        #region RegisterAsync Tests

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ReturnsFail()
        {
            // Arrange
            var registerDto = new RegisterStudentDto { Email = "existing@test.com", Password = "Pass" };
            var existingUser = new User { Email = registerDto.Email };

            _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
                            .ReturnsAsync(existingUser);

            // Act
            var result = await _authManager.RegisterAsync(registerDto);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Errors.Should().Contain("Bu e-posta adresiyle kayıtlı kullanıcı zaten var");
        }

        [Fact]
        public async Task RegisterAsync_UserCreationFails_ReturnsFail()
        {
            // Arrange
            var registerDto = new RegisterStudentDto { Email = "new@test.com", Password = "Pass" };

            _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
                            .ReturnsAsync((User?)null);

            // Mock Transaction
            var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
            _mockUnitOfWork.Setup(x => x.BeginTransactionAsync(default))
                           .ReturnsAsync(mockTransaction.Object);

            // Mock Mapper
            _mockMapper.Setup(x => x.Map<User>(registerDto)).Returns(new User { Email = registerDto.Email });

            // Mock CreateAsync Fail
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), registerDto.Password))
                            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password weak" }));

            // Act
            var result = await _authManager.RegisterAsync(registerDto);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Errors.Should().Contain("Password weak");

            // Verify Rollback
            mockTransaction.Verify(x => x.RollbackAsync(default), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ExceptionThrown_ReturnsFail()
        {
             // Arrange
            var registerDto = new RegisterStudentDto { Email = "new@test.com", Password = "Pass" };

            _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
                            .ReturnsAsync((User?)null);

            // Mock Transaction
            var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
            _mockUnitOfWork.Setup(x => x.BeginTransactionAsync(default))
                           .ReturnsAsync(mockTransaction.Object);

            // Mock Mapper throws exception
            _mockMapper.Setup(x => x.Map<User>(registerDto)).Throws(new Exception("Database error"));

            // Act
            var result = await _authManager.RegisterAsync(registerDto);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(500);
            result.Errors.Should().ContainMatch("Registration failed: Database error");

             // Verify Rollback
            mockTransaction.Verify(x => x.RollbackAsync(default), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_Success_ReturnsToken()
        {
             // Arrange
            var registerDto = new RegisterStudentDto {
                Email = "new@test.com",
                Password = "Pass",
                StudentNumber = "123",
                DepartmentId = 1
            };
            var user = new User { Id = "u1", Email = registerDto.Email };
            var tokenDto = new TokenDto { AccessToken = "acc", RefreshToken = "ref" };

            _mockUserManager.Setup(x => x.FindByEmailAsync(registerDto.Email))
                            .ReturnsAsync((User?)null);

            // Mock Transaction
            var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
            _mockUnitOfWork.Setup(x => x.BeginTransactionAsync(default))
                           .ReturnsAsync(mockTransaction.Object);

            _mockMapper.Setup(x => x.Map<User>(registerDto)).Returns(user);

            // Create Success
            _mockUserManager.Setup(x => x.CreateAsync(user, registerDto.Password))
                            .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.AddToRoleAsync(user, "Student"))
                            .ReturnsAsync(IdentityResult.Success);

            // Mock Repositories setup
            // (Already setup in constructor, but ensuring specific behavior here if needed)

            // Mock Token Generator
            _mockTokenGenerator.Setup(x => x.GenerateToken(user, It.Is<List<string>>(r => r.Contains("Student"))))
                               .Returns(tokenDto);

            // Mock Email Generation
            _mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(user)).ReturnsAsync("email_token");

            // Mock Configuration
            _mockConfiguration.Setup(x => x["ClientSettings:Url"]).Returns("http://test.com");

            // Act
            var result = await _authManager.RegisterAsync(registerDto);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(201);
            result.Data.Should().BeEquivalentTo(tokenDto);

            // Verify Commit
            mockTransaction.Verify(x => x.CommitAsync(default), Times.Once);
            _mockNotificationService.Verify(x => x.SendEmailVerificationAsync(user.Email, It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region VerifyEmailAsync Tests

        [Fact]
        public async Task VerifyEmailAsync_UserNotFound_ReturnsFail()
        {
            // Arrange
            _mockUserManager.Setup(x => x.FindByIdAsync("u1")).ReturnsAsync((User?)null);

            // Act
            var result = await _authManager.VerifyEmailAsync("u1", "token");

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
            result.Errors.Should().Contain("Kullanıcı bulunamadı");
        }

        [Fact]
        public async Task VerifyEmailAsync_ConfirmationFails_ReturnsFail()
        {
            // Arrange
            var user = new User { Id = "u1" };
            _mockUserManager.Setup(x => x.FindByIdAsync("u1")).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.ConfirmEmailAsync(user, "token"))
                            .ReturnsAsync(IdentityResult.Failed());

            // Act
            var result = await _authManager.VerifyEmailAsync("u1", "token");

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Errors.Should().Contain("E-posta doğrulama başarısız");
        }

        [Fact]
        public async Task VerifyEmailAsync_Success_ActivatesUser()
        {
            // Arrange
            var user = new User { Id = "u1", IsActive = false };
            _mockUserManager.Setup(x => x.FindByIdAsync("u1")).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.ConfirmEmailAsync(user, "token"))
                            .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(x => x.UpdateAsync(user))
                            .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authManager.VerifyEmailAsync("u1", "token");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            user.IsActive.Should().BeTrue();

            _mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);
        }

        #endregion

        #region CreateTokenByRefreshTokenAsync Tests

        [Fact]
        public async Task CreateTokenByRefreshTokenAsync_TokenNotFound_ReturnsFail()
        {
            // Arrange
            // Setup Where to return empty list
            var emptyList = new List<RefreshToken>();
            var asyncEnumerable = new TestAsyncEnumerable<RefreshToken>(emptyList);

            _mockRefreshTokenDal.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                                .Returns(asyncEnumerable);

            // Act
            var result = await _authManager.CreateTokenByRefreshTokenAsync("invalid_token");

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
            result.Errors.Should().Contain("Token bulunamadı");
        }

        [Fact]
        public async Task CreateTokenByRefreshTokenAsync_TokenInvalid_ReturnsFail()
        {
            // Arrange
            var refreshToken = new RefreshToken
            {
                Token = "expired_token",
                Expires = DateTime.UtcNow.AddMinutes(-10), // Expired
                Revoked = null
            };
            var list = new List<RefreshToken> { refreshToken };
            var asyncEnumerable = new TestAsyncEnumerable<RefreshToken>(list);

             _mockRefreshTokenDal.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                                .Returns(asyncEnumerable);

            // Act
            var result = await _authManager.CreateTokenByRefreshTokenAsync("expired_token");

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Errors.Should().Contain("Token aktif değil");
        }

        [Fact]
        public async Task CreateTokenByRefreshTokenAsync_UserNotFound_ReturnsFail()
        {
            // Arrange
            var refreshToken = new RefreshToken
            {
                UserId = "u1",
                Token = "valid_token",
                Expires = DateTime.UtcNow.AddMinutes(10),
                Revoked = null
            };
            var list = new List<RefreshToken> { refreshToken };
            var asyncEnumerable = new TestAsyncEnumerable<RefreshToken>(list);

             _mockRefreshTokenDal.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                                .Returns(asyncEnumerable);

            _mockUserManager.Setup(x => x.FindByIdAsync("u1")).ReturnsAsync((User?)null);

            // Act
            var result = await _authManager.CreateTokenByRefreshTokenAsync("valid_token");

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
            result.Errors.Should().Contain("Kullanıcı bulunamadı");
        }

        [Fact]
        public async Task CreateTokenByRefreshTokenAsync_Success_ReturnsNewToken()
        {
            // Arrange
            var refreshToken = new RefreshToken
            {
                UserId = "u1",
                Token = "valid_token",
                Expires = DateTime.UtcNow.AddMinutes(10),
                Revoked = null
            };
            var list = new List<RefreshToken> { refreshToken };
            var asyncEnumerable = new TestAsyncEnumerable<RefreshToken>(list);

             _mockRefreshTokenDal.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                                .Returns(asyncEnumerable);

            var user = new User { Id = "u1", Email = "test@test.com" };
            _mockUserManager.Setup(x => x.FindByIdAsync("u1")).ReturnsAsync(user);

            var roles = new List<string> { "Student" };
            _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(roles);

            var newTokenDto = new TokenDto { AccessToken = "new_access", RefreshToken = "new_refresh" };
            _mockTokenGenerator.Setup(x => x.GenerateToken(user, roles)).Returns(newTokenDto);

            // Act
            var result = await _authManager.CreateTokenByRefreshTokenAsync("valid_token");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(newTokenDto);

            // Verify old token revoked
            refreshToken.Revoked.Should().NotBeNull();
            refreshToken.ReasonRevoked.Should().Be("Replaced by new token");
            _mockRefreshTokenDal.Verify(x => x.Update(refreshToken), Times.Once);

            // Verify new token added
            _mockRefreshTokenDal.Verify(x => x.AddAsync(It.Is<RefreshToken>(rt => rt.Token == newTokenDto.RefreshToken)), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        }

        #endregion

        #region RevokeRefreshTokenAsync Tests

        [Fact]
        public async Task RevokeRefreshTokenAsync_NotFound_ReturnsFail()
        {
            // Arrange
            var emptyList = new List<RefreshToken>();
            var asyncEnumerable = new TestAsyncEnumerable<RefreshToken>(emptyList);

            _mockRefreshTokenDal.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                                .Returns(asyncEnumerable);

            // Act
            var result = await _authManager.RevokeRefreshTokenAsync("invalid");

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task RevokeRefreshTokenAsync_Success_RevokesToken()
        {
            // Arrange
             var refreshToken = new RefreshToken
            {
                Token = "valid_token",
                Expires = DateTime.UtcNow.AddMinutes(10),
                Revoked = null
            };
            var list = new List<RefreshToken> { refreshToken };
            var asyncEnumerable = new TestAsyncEnumerable<RefreshToken>(list);

             _mockRefreshTokenDal.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<Func<RefreshToken, bool>>>()))
                                .Returns(asyncEnumerable);

            // Act
            var result = await _authManager.RevokeRefreshTokenAsync("valid_token");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            refreshToken.Revoked.Should().NotBeNull();
            refreshToken.ReasonRevoked.Should().Be("Revoked by user");

            _mockRefreshTokenDal.Verify(x => x.Update(refreshToken), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        }

        #endregion

        #region ForgotPasswordAsync Tests

        [Fact]
        public async Task ForgotPasswordAsync_UserNotFound_ReturnsSuccess()
        {
            // Arrange
            _mockUserManager.Setup(x => x.FindByEmailAsync("unknown@test.com"))
                            .ReturnsAsync((User?)null);

            // Act
            var result = await _authManager.ForgotPasswordAsync(new ForgotPasswordDto { Email = "unknown@test.com" });

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(200);

            // Should not send email or add token
            _mockPasswordResetTokenDal.Verify(x => x.AddAsync(It.IsAny<PasswordResetToken>()), Times.Never);
            _mockNotificationService.Verify(x => x.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ForgotPasswordAsync_Success_SendsEmail()
        {
            // Arrange
             var user = new User { Id = "u1", Email = "user@test.com" };
            _mockUserManager.Setup(x => x.FindByEmailAsync(user.Email))
                            .ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset_token");
            _mockConfiguration.Setup(x => x["ClientSettings:Url"]).Returns("http://test.com");

            // Act
            var result = await _authManager.ForgotPasswordAsync(new ForgotPasswordDto { Email = user.Email });

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(200);

            _mockPasswordResetTokenDal.Verify(x => x.AddAsync(It.Is<PasswordResetToken>(t => t.UserId == user.Id && t.Token == "reset_token")), Times.Once);
            _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
            _mockNotificationService.Verify(x => x.SendPasswordResetEmailAsync(user.Email, It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region ResetPasswordAsync Tests

        [Fact]
        public async Task ResetPasswordAsync_UserNotFound_ReturnsFail()
        {
            // Arrange
             _mockUserManager.Setup(x => x.FindByEmailAsync("unknown@test.com"))
                            .ReturnsAsync((User?)null);

            // Act
            var result = await _authManager.ResetPasswordAsync(new ResetPasswordDto { Email = "unknown@test.com", Token = "t", NewPassword = "p" });

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Errors.Should().Contain("Invalid request");
        }

        [Fact]
        public async Task ResetPasswordAsync_ResetFails_ReturnsFail()
        {
            // Arrange
            var user = new User { Email = "user@test.com" };
             _mockUserManager.Setup(x => x.FindByEmailAsync(user.Email))
                            .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.ResetPasswordAsync(user, "token", "newPass"))
                            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

            // Act
            var result = await _authManager.ResetPasswordAsync(new ResetPasswordDto { Email = user.Email, Token = "token", NewPassword = "newPass" });

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Errors.Should().Contain("Error");
        }

        [Fact]
        public async Task ResetPasswordAsync_Success_ReturnsSuccess()
        {
            // Arrange
            var user = new User { Email = "user@test.com" };
             _mockUserManager.Setup(x => x.FindByEmailAsync(user.Email))
                            .ReturnsAsync(user);

            _mockUserManager.Setup(x => x.ResetPasswordAsync(user, "token", "newPass"))
                            .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authManager.ResetPasswordAsync(new ResetPasswordDto { Email = user.Email, Token = "token", NewPassword = "newPass" });

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(200);
        }

        #endregion

    }
}
