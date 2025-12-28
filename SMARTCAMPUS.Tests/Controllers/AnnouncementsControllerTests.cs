using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.BusinessLayer.Tools;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;
using SMARTCAMPUS.EntityLayer.Models;
using SMARTCAMPUS.Tests.TestUtilities;
using System.Security.Claims;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class AnnouncementsControllerTests
    {
        private readonly Mock<IAnnouncementService> _mockService;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly UserClaimsHelper _userClaimsHelper;
        private readonly AnnouncementsController _controller;

        public AnnouncementsControllerTests()
        {
            _mockService = new Mock<IAnnouncementService>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _userClaimsHelper = new UserClaimsHelper(_mockHttpContextAccessor.Object, _mockUnitOfWork.Object);
            _controller = new AnnouncementsController(_mockService.Object, _userClaimsHelper);
            SetupHttpContext("user1");
        }

        private void SetupHttpContext(string userId)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task GetAnnouncements_ShouldReturnOk()
        {
            var mockStudent = new Mock<IStudentDal>();
            var students = new List<Student>();
            var asyncEnumerable = new TestAsyncEnumerable<Student>(students);
            _mockUnitOfWork.Setup(x => x.Students).Returns(mockStudent.Object);
            mockStudent.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<Func<Student, bool>>>()))
                .Returns(asyncEnumerable);

            _mockService.Setup(x => x.GetAnnouncementsAsync(null, null))
                .ReturnsAsync(Response<IEnumerable<AnnouncementDto>>.Success(new List<AnnouncementDto>(), 200));

            var result = await _controller.GetAnnouncements(null, null);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetAnnouncements_ShouldUseStudentDepartment_WhenStudent()
        {
            var mockStudent = new Mock<IStudentDal>();
            var student = new Student { Id = 1, UserId = "user1", DepartmentId = 5, IsActive = true };
            var students = new List<Student> { student };
            var asyncEnumerable = new TestAsyncEnumerable<Student>(students);
            _mockUnitOfWork.Setup(x => x.Students).Returns(mockStudent.Object);
            mockStudent.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<Func<Student, bool>>>()))
                .Returns(asyncEnumerable);
            mockStudent.Setup(x => x.GetStudentWithDetailsAsync(1))
                .ReturnsAsync(student);

            _mockService.Setup(x => x.GetAnnouncementsAsync(null, 5))
                .ReturnsAsync(Response<IEnumerable<AnnouncementDto>>.Success(new List<AnnouncementDto>(), 200));

            var result = await _controller.GetAnnouncements(null, null);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetImportantAnnouncements_ShouldReturnOk()
        {
            _mockService.Setup(x => x.GetImportantAnnouncementsAsync())
                .ReturnsAsync(Response<IEnumerable<AnnouncementDto>>.Success(new List<AnnouncementDto>(), 200));

            var result = await _controller.GetImportantAnnouncements();

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetAnnouncementById_ShouldReturnOk()
        {
            _mockService.Setup(x => x.GetAnnouncementByIdAsync(1))
                .ReturnsAsync(Response<AnnouncementDto>.Success(new AnnouncementDto(), 200));

            var result = await _controller.GetAnnouncementById(1);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task IncrementViewCount_ShouldReturnOk()
        {
            _mockService.Setup(x => x.IncrementViewCountAsync(1))
                .ReturnsAsync(Response<EntityLayer.DTOs.NoDataDto>.Success(200));

            var result = await _controller.IncrementViewCount(1);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }
    }
}
