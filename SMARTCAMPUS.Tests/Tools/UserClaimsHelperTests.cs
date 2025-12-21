using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SMARTCAMPUS.BusinessLayer.Tools;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.Models;
using Xunit;

namespace SMARTCAMPUS.Tests.Tools
{
    public class UserClaimsHelperTests
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly UserClaimsHelper _helper;
        private readonly DefaultHttpContext _httpContext;

        public UserClaimsHelperTests()
        {
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _httpContext = new DefaultHttpContext();
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_httpContext);
            _helper = new UserClaimsHelper(_mockHttpContextAccessor.Object, _mockUnitOfWork.Object);
        }

        private void SetupUser(params Claim[] claims)
        {
            var identity = new ClaimsIdentity(claims, "test");
            _httpContext.User = new ClaimsPrincipal(identity);
        }

        #region GetUserId Tests

        [Fact]
        public void GetUserId_ShouldReturnNameIdentifier()
        {
            // Arrange
            SetupUser(new Claim(ClaimTypes.NameIdentifier, "user-123"));

            // Act
            var result = _helper.GetUserId();

            // Assert
            result.Should().Be("user-123");
        }

        [Fact]
        public void GetUserId_ShouldReturnSubClaim_WhenNoNameIdentifier()
        {
            // Arrange
            SetupUser(new Claim("sub", "user-456"));

            // Act
            var result = _helper.GetUserId();

            // Assert
            result.Should().Be("user-456");
        }

        [Fact]
        public void GetUserId_ShouldReturnNameClaim_WhenNoOthers()
        {
            // Arrange
            SetupUser(new Claim(ClaimTypes.Name, "user-789"));

            // Act
            var result = _helper.GetUserId();

            // Assert
            result.Should().Be("user-789");
        }

        [Fact]
        public void GetUserId_ShouldReturnNull_WhenNoUserClaims()
        {
            // Arrange
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var result = _helper.GetUserId();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetUserId_ShouldPreferNameIdentifier_OverOtherClaims()
        {
            // Arrange
            SetupUser(
                new Claim(ClaimTypes.NameIdentifier, "preferred-id"),
                new Claim("sub", "fallback-id"),
                new Claim(ClaimTypes.Name, "name-id")
            );

            // Act
            var result = _helper.GetUserId();

            // Assert
            result.Should().Be("preferred-id");
        }

        #endregion

        #region GetUserEmail Tests

        [Fact]
        public void GetUserEmail_ShouldReturnEmailClaim()
        {
            // Arrange
            SetupUser(new Claim(ClaimTypes.Email, "test@example.com"));

            // Act
            var result = _helper.GetUserEmail();

            // Assert
            result.Should().Be("test@example.com");
        }

        [Fact]
        public void GetUserEmail_ShouldReturnCustomEmailClaim()
        {
            // Arrange
            SetupUser(new Claim("email", "custom@example.com"));

            // Act
            var result = _helper.GetUserEmail();

            // Assert
            result.Should().Be("custom@example.com");
        }

        [Fact]
        public void GetUserEmail_ShouldReturnNull_WhenNoEmailClaim()
        {
            // Arrange
            SetupUser(new Claim(ClaimTypes.Name, "some-user"));

            // Act
            var result = _helper.GetUserEmail();

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region IsInRole Tests

        [Fact]
        public void IsInRole_ShouldReturnTrue_WhenUserHasRole()
        {
            // Arrange
            SetupUser(new Claim(ClaimTypes.Role, "Admin"));

            // Act
            var result = _helper.IsInRole("Admin");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsInRole_ShouldReturnFalse_WhenUserDoesNotHaveRole()
        {
            // Arrange
            SetupUser(new Claim(ClaimTypes.Role, "Student"));

            // Act
            var result = _helper.IsInRole("Admin");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsInRole_ShouldReturnFalse_WhenNoHttpContext()
        {
            // Arrange
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);
            var helper = new UserClaimsHelper(_mockHttpContextAccessor.Object, _mockUnitOfWork.Object);

            // Act
            var result = helper.IsInRole("Admin");

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region GetRoles Tests

        [Fact]
        public void GetRoles_ShouldReturnAllRoles()
        {
            // Arrange
            SetupUser(
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.Role, "Faculty")
            );

            // Act
            var result = _helper.GetRoles();

            // Assert
            result.Should().ContainInOrder("Admin", "Faculty");
        }

        [Fact]
        public void GetRoles_ShouldReturnEmpty_WhenNoRoles()
        {
            // Arrange
            SetupUser(new Claim(ClaimTypes.Name, "user"));

            // Act
            var result = _helper.GetRoles();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void GetRoles_ShouldReturnEmpty_WhenNoHttpContext()
        {
            // Arrange
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);
            var helper = new UserClaimsHelper(_mockHttpContextAccessor.Object, _mockUnitOfWork.Object);

            // Act
            var result = helper.GetRoles();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetStudentIdAsync Tests

        [Fact]
        public async Task GetStudentIdAsync_ShouldReturnNull_WhenNoUserId()
        {
            // Arrange
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var result = await _helper.GetStudentIdAsync();

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetFacultyIdAsync Tests

        [Fact]
        public async Task GetFacultyIdAsync_ShouldReturnNull_WhenNoUserId()
        {
            // Arrange
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var result = await _helper.GetFacultyIdAsync();

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetStudentWithDetailsAsync Tests

        [Fact]
        public async Task GetStudentWithDetailsAsync_ShouldReturnStudent_WhenExists()
        {
            // Arrange
            var student = new Student { Id = 10, StudentNumber = "S123" };
            var mockStudentsRepo = new Mock<IStudentDal>();
            mockStudentsRepo.Setup(x => x.GetStudentWithDetailsAsync(10)).ReturnsAsync(student);
            _mockUnitOfWork.Setup(x => x.Students).Returns(mockStudentsRepo.Object);

            // Act
            var result = await _helper.GetStudentWithDetailsAsync(10);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(10);
            result.StudentNumber.Should().Be("S123");
        }

        [Fact]
        public async Task GetStudentWithDetailsAsync_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            var mockStudentsRepo = new Mock<IStudentDal>();
            mockStudentsRepo.Setup(x => x.GetStudentWithDetailsAsync(999)).ReturnsAsync((Student?)null);
            _mockUnitOfWork.Setup(x => x.Students).Returns(mockStudentsRepo.Object);

            // Act
            var result = await _helper.GetStudentWithDetailsAsync(999);

            // Assert
            result.Should().BeNull();
        }

        #endregion
    }
}

