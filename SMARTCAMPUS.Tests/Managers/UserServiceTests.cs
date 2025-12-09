using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs.User;
using SMARTCAMPUS.EntityLayer.Models;
using SMARTCAMPUS.Tests.TestUtilities;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class UserServiceTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IMapper> _mockMapper;
        private readonly DbContextOptions<CampusContext> _dbContextOptions;

        public UserServiceTests()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            _mockMapper = new Mock<IMapper>();

            _dbContextOptions = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnFail_WhenNotFound()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, context);

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync((User?)null);

            // Act
            var result = await userService.GetUserByIdAsync("1");

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnSuccess_WhenFound()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, context);

            var user = new User { Id = "1", FullName = "Test" };
            var dto = new UserProfileDto { Id = "1", FullName = "Test" };

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
            _mockMapper.Setup(x => x.Map<UserProfileDto>(user)).Returns(dto);

            // Act
            var result = await userService.GetUserByIdAsync("1");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnSuccess_WhenUpdated()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, context);

            var user = new User { Id = "1", FullName = "Old" };
            var updateDto = new UserUpdateDto { FullName = "New", Email = "new@test.com" };

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await userService.UpdateUserAsync("1", updateDto);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            user.FullName.Should().Be("New");
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldSoftDelete_AndRevokeTokens()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);

            // Seed refresh token
            var token = new RefreshToken { Token = "t1", UserId = "1", Revoked = null };
            await context.RefreshTokens.AddAsync(token);
            await context.SaveChangesAsync();

            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, context);
            var user = new User { Id = "1", IsActive = true };

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await userService.DeleteUserAsync("1");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            user.IsActive.Should().BeFalse();

            var dbToken = await context.RefreshTokens.FirstAsync();
            dbToken.Revoked.Should().NotBeNull();
            dbToken.ReasonRevoked.Should().Contain("Soft Delete");
        }
    }
}
