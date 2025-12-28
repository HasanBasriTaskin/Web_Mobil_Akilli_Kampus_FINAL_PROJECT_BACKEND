using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.BusinessLayer.Tools;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;
using SMARTCAMPUS.EntityLayer.Models;
using SMARTCAMPUS.Tests.TestUtilities;
using System.Security.Claims;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class ExcuseRequestsControllerTests
    {
        private readonly Mock<IExcuseRequestService> _mockExcuseService;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly UserClaimsHelper _userClaimsHelper;
        private readonly ExcuseRequestsController _controller;

        public ExcuseRequestsControllerTests()
        {
            _mockExcuseService = new Mock<IExcuseRequestService>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _userClaimsHelper = new UserClaimsHelper(_mockHttpContextAccessor.Object, _mockUnitOfWork.Object);
            _controller = new ExcuseRequestsController(_mockExcuseService.Object, _userClaimsHelper);
            SetupHttpContext("user1");
        }

        private void SetupHttpContext(string? userId, string? role = null)
        {
            var claims = new List<Claim>();
            if (!string.IsNullOrEmpty(userId))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            }
            if (!string.IsNullOrEmpty(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task CreateExcuseRequest_ShouldReturnOk()
        {
            SetupHttpContext("user1");
            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.User = _controller.ControllerContext.HttpContext.User;
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext);
            
            
            var mockStudent = new Mock<IStudentDal>();
            var students = new List<Student>
            {
                new Student { Id = 1, UserId = "user1", IsActive = true }
            };
            var asyncEnumerable = new TestAsyncEnumerable<Student>(students);
            _mockUnitOfWork.Setup(x => x.Students).Returns(mockStudent.Object);
            mockStudent.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<Func<Student, bool>>>()))
                .Returns(asyncEnumerable);

            var dto = new ExcuseRequestCreateDto { SessionId = 1, Reason = "Test", DocumentUrl = "url" };
            _mockExcuseService.Setup(x => x.CreateExcuseRequestAsync(1, dto))
                .ReturnsAsync(Response<ExcuseRequestDto>.Success(new ExcuseRequestDto(), 201));

            var result = await _controller.CreateExcuseRequest(dto);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task CreateExcuseRequest_ShouldReturnUnauthorized_WhenNotStudent()
        {
            SetupHttpContext("user1");
            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.User = _controller.ControllerContext.HttpContext.User;
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext);
            
            var mockStudent = new Mock<IStudentDal>();
            var students = new List<Student>();
            var asyncEnumerable = new TestAsyncEnumerable<Student>(students);
            _mockUnitOfWork.Setup(x => x.Students).Returns(mockStudent.Object);
            mockStudent.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<Func<Student, bool>>>()))
                .Returns(asyncEnumerable);

            var dto = new ExcuseRequestCreateDto { SessionId = 1, Reason = "Test" };
            var result = await _controller.CreateExcuseRequest(dto);

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task GetExcuseRequests_ShouldReturnOk()
        {
            SetupHttpContext("user1", "Faculty");
            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.User = _controller.ControllerContext.HttpContext.User;
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext);
            
            _mockExcuseService.Setup(x => x.GetExcuseRequestsAsync("user1"))
                .ReturnsAsync(Response<IEnumerable<ExcuseRequestDto>>.Success(new List<ExcuseRequestDto>(), 200));

            var result = await _controller.GetExcuseRequests();

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task ApproveExcuseRequest_ShouldReturnOk()
        {
            SetupHttpContext("user1", "Faculty");
            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.User = _controller.ControllerContext.HttpContext.User;
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext);
            
            var dto = new ExcuseRequestReviewDto { Notes = "Approved" };
            _mockExcuseService.Setup(x => x.ApproveExcuseRequestAsync(1, "user1", "Approved"))
                .ReturnsAsync(Response<NoDataDto>.Success(200));

            var result = await _controller.ApproveExcuseRequest(1, dto);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task ApproveExcuseRequest_ShouldReturnUnauthorized_WhenInstructorIdIsNull()
        {
            SetupHttpContext(null, "Faculty");
            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.User = _controller.ControllerContext.HttpContext.User;
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext);
            
            var dto = new ExcuseRequestReviewDto { Notes = "Approved" };

            var result = await _controller.ApproveExcuseRequest(1, dto);

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task RejectExcuseRequest_ShouldReturnOk()
        {
            SetupHttpContext("user1", "Faculty");
            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.User = _controller.ControllerContext.HttpContext.User;
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext);
            
            var dto = new ExcuseRequestReviewDto { Notes = "Rejected" };
            _mockExcuseService.Setup(x => x.RejectExcuseRequestAsync(1, "user1", "Rejected"))
                .ReturnsAsync(Response<NoDataDto>.Success(200));

            var result = await _controller.RejectExcuseRequest(1, dto);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task RejectExcuseRequest_ShouldReturnUnauthorized_WhenInstructorIdIsNull()
        {
            SetupHttpContext(null, "Faculty");
            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.User = _controller.ControllerContext.HttpContext.User;
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext);
            
            var dto = new ExcuseRequestReviewDto { Notes = "Rejected" };

            var result = await _controller.RejectExcuseRequest(1, dto);

            result.Should().BeOfType<UnauthorizedObjectResult>();
        }
    }
}
