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
using System.IO;
using Xunit;
using SMARTCAMPUS.DataAccessLayer.Concrete;

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

        #region GetUserByIdAsync Tests

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnFail_WhenNotFound()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));

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
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));

            var user = new User { Id = "1", FullName = "Test", Email = "test@test.com" };

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Student" });

            // Act
            var result = await userService.GetUserByIdAsync("1");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Id.Should().Be("1");
            result.Data.FullName.Should().Be("Test");
            result.Data.Roles.Should().Contain("Student");
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnStudentInfo_WhenUserIsStudent()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            
            // Seed student data
            var student = new Student { UserId = "1", StudentNumber = "STU001", DepartmentId = 1 };
            await context.Students.AddAsync(student);
            await context.SaveChangesAsync();

            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));

            var user = new User { Id = "1", FullName = "Test", Email = "test@test.com" };

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Student" });

            // Act
            var result = await userService.GetUserByIdAsync("1");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Student.Should().NotBeNull();
            result.Data.Student.StudentNumber.Should().Be("STU001");
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnFacultyInfo_WhenUserIsFaculty()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            
            // Seed faculty data
            var faculty = new Faculty { UserId = "1", EmployeeNumber = "FAC001", Title = "Dr.", DepartmentId = 1, OfficeLocation = "A101" };
            await context.Faculties.AddAsync(faculty);
            await context.SaveChangesAsync();

            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));

            var user = new User { Id = "1", FullName = "Test Faculty", Email = "faculty@test.com" };

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Faculty" });

            // Act
            var result = await userService.GetUserByIdAsync("1");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Faculty.Should().NotBeNull();
            result.Data.Faculty.EmployeeNumber.Should().Be("FAC001");
            result.Data.Faculty.Title.Should().Be("Dr.");
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnDefaultRole_WhenNoRolesAssigned()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));

            var user = new User { Id = "1", FullName = "Test", Email = "test@test.com" };

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string>());

            // Act
            var result = await userService.GetUserByIdAsync("1");

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Role.Should().Be("User");
        }

        #endregion

        #region UpdateUserAsync Tests

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnSuccess_WhenUpdated()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));

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
        public async Task UpdateUserAsync_ShouldReturnFail_WhenUserNotFound()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));

            var updateDto = new UserUpdateDto { FullName = "New", Email = "new@test.com" };

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync((User?)null);

            // Act
            var result = await userService.UpdateUserAsync("1", updateDto);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnFail_WhenUpdateFails()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));

            var user = new User { Id = "1", FullName = "Old" };
            var updateDto = new UserUpdateDto { FullName = "New", Email = "new@test.com" };

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));

            // Act
            var result = await userService.UpdateUserAsync("1", updateDto);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Errors.Should().Contain("Update failed");
        }

        #endregion

        #region DeleteUserAsync Tests

        [Fact]
        public async Task DeleteUserAsync_ShouldSoftDelete_AndRevokeTokens()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);

            // Seed refresh token
            var token = new RefreshToken { Token = "t1", UserId = "1", Revoked = null };
            await context.RefreshTokens.AddAsync(token);
            await context.SaveChangesAsync();

            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));
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

        [Fact]
        public async Task DeleteUserAsync_ShouldReturnFail_WhenUserNotFound()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync((User?)null);

            // Act
            var result = await userService.DeleteUserAsync("1");

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldReturnFail_WhenUpdateFails()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));
            var user = new User { Id = "1", IsActive = true };

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Delete failed" }));

            // Act
            var result = await userService.DeleteUserAsync("1");

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        #endregion

        #region AssignRolesAsync Tests

        [Fact]
        public async Task AssignRolesAsync_ShouldReturnSuccess_WhenRolesAssigned()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));
            var user = new User { Id = "1", FullName = "Test" };
            var newRoles = new List<string> { "Admin", "Faculty" };

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Student" });
            _mockUserManager.Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(x => x.AddToRolesAsync(user, newRoles)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await userService.AssignRolesAsync("1", newRoles);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task AssignRolesAsync_ShouldReturnFail_WhenUserNotFound()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync((User?)null);

            // Act
            var result = await userService.AssignRolesAsync("1", new List<string> { "Admin" });

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task AssignRolesAsync_ShouldReturnFail_WhenRemoveRolesFails()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));
            var user = new User { Id = "1", FullName = "Test" };

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Student" });
            _mockUserManager.Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Remove failed" }));

            // Act
            var result = await userService.AssignRolesAsync("1", new List<string> { "Admin" });

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task AssignRolesAsync_ShouldReturnFail_WhenAddRolesFails()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));
            var user = new User { Id = "1", FullName = "Test" };
            var newRoles = new List<string> { "Admin" };

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Student" });
            _mockUserManager.Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(x => x.AddToRolesAsync(user, newRoles))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Add failed" }));

            // Act
            var result = await userService.AssignRolesAsync("1", newRoles);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        #endregion

        #region UploadProfilePictureAsync Tests

        [Fact]
        public async Task UploadProfilePictureAsync_ShouldReturnFail_WhenUserNotFound()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync((User?)null);

            var fileMock = new Mock<IFormFile>();

            // Act
            var result = await userService.UploadProfilePictureAsync("1", fileMock.Object);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        [Fact]
        public async Task UploadProfilePictureAsync_ShouldReturnFail_WhenFileIsNull()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));
            var user = new User { Id = "1", FullName = "Test" };

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);

            // Act
            var result = await userService.UploadProfilePictureAsync("1", null!);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Errors.Should().Contain("Dosya boş");
        }

        [Fact]
        public async Task UploadProfilePictureAsync_ShouldReturnFail_WhenFileIsEmpty()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));
            var user = new User { Id = "1", FullName = "Test" };

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(0);

            // Act
            var result = await userService.UploadProfilePictureAsync("1", fileMock.Object);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task UploadProfilePictureAsync_ShouldReturnFail_WhenFileTooLarge()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));
            var user = new User { Id = "1", FullName = "Test" };

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(10 * 1024 * 1024); // 10MB
            fileMock.Setup(f => f.FileName).Returns("test.jpg");

            // Act
            var result = await userService.UploadProfilePictureAsync("1", fileMock.Object);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Errors.Should().Contain("Dosya boyutu en fazla 5MB olabilir");
        }

        [Fact]
        public async Task UploadProfilePictureAsync_ShouldReturnFail_WhenInvalidFileType()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));
            var user = new User { Id = "1", FullName = "Test" };

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.FileName).Returns("test.gif");

            // Act
            var result = await userService.UploadProfilePictureAsync("1", fileMock.Object);

            // Assert
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Errors.Should().Contain("Geçersiz dosya tipi");
        }

        [Fact]
        public async Task UploadProfilePictureAsync_ShouldReturnSuccess_WhenValidJpgFile()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));
            var user = new User { Id = "1", FullName = "Test" };

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Create a mock file
            var content = "fake image content";
            var fileName = "test.jpg";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await userService.UploadProfilePictureAsync("1", fileMock.Object);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.Data.Should().Contain("/uploads/profile-pictures/");
            result.Data.Should().Contain(".jpg");
        }

        [Fact]
        public async Task UploadProfilePictureAsync_ShouldReturnSuccess_WhenValidPngFile()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));
            var user = new User { Id = "1", FullName = "Test" };

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var content = "fake image content";
            var fileName = "test.png";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await userService.UploadProfilePictureAsync("1", fileMock.Object);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.Data.Should().Contain(".png");
        }

        [Fact]
        public async Task UploadProfilePictureAsync_ShouldReturnSuccess_WhenValidJpegFile()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));
            var user = new User { Id = "1", FullName = "Test" };

            _mockUserManager.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(user);
            _mockUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var content = "fake image content";
            var fileName = "test.jpeg";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await userService.UploadProfilePictureAsync("1", fileMock.Object);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.Data.Should().Contain(".jpeg");
        }

        #endregion

        #region GetUsersAsync Tests

        [Fact]
        public async Task GetUsersAsync_ShouldReturnSuccess_WithEmptyList()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));
            var queryParams = new UserQueryParameters { Page = 1, Limit = 10 };

            // Act
            var result = await userService.GetUsersAsync(queryParams);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.Data.Data.Should().BeEmpty();
            result.Data.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetUsersAsync_ShouldReturnUsers_WithPagination()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            
            // Seed users
            for (int i = 1; i <= 15; i++)
            {
                await context.Users.AddAsync(new User { Id = $"u{i}", FullName = $"User{i}", Email = $"user{i}@test.com" });
            }
            await context.SaveChangesAsync();

            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));
            var queryParams = new UserQueryParameters { Page = 1, Limit = 10 };

            // Act
            var result = await userService.GetUsersAsync(queryParams);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Data.Should().HaveCount(10);
            result.Data.TotalRecords.Should().Be(15);
            result.Data.TotalPages.Should().Be(2);
        }

        [Fact]
        public async Task GetUsersAsync_ShouldFilterBySearch()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            
            await context.Users.AddAsync(new User { Id = "u1", FullName = "John Doe", Email = "john@test.com" });
            await context.Users.AddAsync(new User { Id = "u2", FullName = "Jane Smith", Email = "jane@test.com" });
            await context.SaveChangesAsync();

            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));
            var queryParams = new UserQueryParameters { Page = 1, Limit = 10, Search = "John" };

            // Act
            var result = await userService.GetUsersAsync(queryParams);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Data.Should().HaveCount(1);
            result.Data.Data.First().FullName.Should().Be("John Doe");
        }

        [Fact]
        public async Task GetUsersAsync_ShouldFilterByEmail()
        {
            // Arrange
            using var context = new CampusContext(_dbContextOptions);
            
            await context.Users.AddAsync(new User { Id = "u1", FullName = "John Doe", Email = "john@test.com" });
            await context.Users.AddAsync(new User { Id = "u2", FullName = "Jane Smith", Email = "jane@test.com" });
            await context.SaveChangesAsync();

            var userService = new UserService(_mockUserManager.Object, _mockMapper.Object, new UnitOfWork(context));
            var queryParams = new UserQueryParameters { Page = 1, Limit = 10, Search = "jane@" };

            // Act
            var result = await userService.GetUsersAsync(queryParams);

            // Assert
            result.IsSuccessful.Should().BeTrue();
            result.Data.Data.Should().HaveCount(1);
            result.Data.Data.First().Email.Should().Be("jane@test.com");
        }

        #endregion
    }
}
