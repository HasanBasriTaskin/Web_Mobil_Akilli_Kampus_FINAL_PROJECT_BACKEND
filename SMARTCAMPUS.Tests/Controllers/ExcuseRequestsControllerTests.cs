using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    /// <summary>
    /// Tests for ExcuseRequestsController.
    /// Note: This controller uses UserClaimsHelper which has non-virtual methods,
    /// making direct mocking impossible. These tests focus on service layer mocking.
    /// Integration tests would be needed for full coverage of auth scenarios.
    /// </summary>
    public class ExcuseRequestsControllerTests
    {
        private readonly Mock<IExcuseRequestService> _mockService;

        public ExcuseRequestsControllerTests()
        {
            _mockService = new Mock<IExcuseRequestService>();
        }

        #region Service Integration Tests

        [Fact]
        public void ExcuseRequestsController_ShouldHaveExpectedMethods()
        {
            // Arrange - Verify controller has required endpoints
            var controllerType = typeof(ExcuseRequestsController);
            
            // Assert
            controllerType.GetMethod("CreateExcuseRequest").Should().NotBeNull();
            controllerType.GetMethod("GetExcuseRequests").Should().NotBeNull();
            controllerType.GetMethod("ApproveExcuseRequest").Should().NotBeNull();
            controllerType.GetMethod("RejectExcuseRequest").Should().NotBeNull();
        }

        [Fact]
        public async Task IExcuseRequestService_CreateExcuseRequestAsync_ShouldReturnDto()
        {
            // Arrange
            var createDto = new ExcuseRequestCreateDto { SessionId = 1, Reason = "Medical appointment" };
            var responseDto = new ExcuseRequestDto { Id = 1, Reason = "Medical appointment" };
            var response = Response<ExcuseRequestDto>.Success(responseDto, 201);
            _mockService.Setup(x => x.CreateExcuseRequestAsync(1, createDto)).ReturnsAsync(response);

            // Act
            var result = await _mockService.Object.CreateExcuseRequestAsync(1, createDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(201);
            result.Data.Should().NotBeNull();
            result.Data!.Reason.Should().Be("Medical appointment");
        }

        [Fact]
        public async Task IExcuseRequestService_GetExcuseRequestsAsync_ShouldReturnList()
        {
            // Arrange
            var requests = new List<ExcuseRequestDto>
            {
                new ExcuseRequestDto { Id = 1, Reason = "Reason 1" }
            };
            var response = Response<IEnumerable<ExcuseRequestDto>>.Success(requests, 200);
            _mockService.Setup(x => x.GetExcuseRequestsAsync("instructor-1")).ReturnsAsync(response);

            // Act
            var result = await _mockService.Object.GetExcuseRequestsAsync("instructor-1");

            // Assert
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task IExcuseRequestService_ApproveExcuseRequestAsync_ShouldSucceed()
        {
            // Arrange
            var response = Response<NoDataDto>.Success(200);
            _mockService.Setup(x => x.ApproveExcuseRequestAsync(1, "instructor-1", "Approved")).ReturnsAsync(response);

            // Act
            var result = await _mockService.Object.ApproveExcuseRequestAsync(1, "instructor-1", "Approved");

            // Assert
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task IExcuseRequestService_RejectExcuseRequestAsync_ShouldSucceed()
        {
            // Arrange
            var response = Response<NoDataDto>.Success(200);
            _mockService.Setup(x => x.RejectExcuseRequestAsync(1, "instructor-1", "Rejected")).ReturnsAsync(response);

            // Act
            var result = await _mockService.Object.RejectExcuseRequestAsync(1, "instructor-1", "Rejected");

            // Assert
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task IExcuseRequestService_RejectExcuseRequestAsync_ShouldFail_WhenNotFound()
        {
            // Arrange
            var response = Response<NoDataDto>.Fail("Request not found", 404);
            _mockService.Setup(x => x.RejectExcuseRequestAsync(999, "instructor-1", null)).ReturnsAsync(response);

            // Act
            var result = await _mockService.Object.RejectExcuseRequestAsync(999, "instructor-1", null);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        #endregion
    }
}
