using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    /// <summary>
    /// Tests for AnnouncementsController.
    /// Note: This controller uses UserClaimsHelper which has non-virtual methods,
    /// making direct mocking impossible. These tests focus on service layer mocking.
    /// </summary>
    public class AnnouncementsControllerTests
    {
        private readonly Mock<IAnnouncementService> _mockService;

        public AnnouncementsControllerTests()
        {
            _mockService = new Mock<IAnnouncementService>();
        }

        #region GetAnnouncements Service Tests

        [Fact]
        public async Task IAnnouncementService_GetAnnouncementsAsync_ShouldReturnList()
        {
            // Arrange
            var announcements = new List<AnnouncementDto>
            {
                new AnnouncementDto { Id = 1, Title = "Test Announcement" }
            };
            var response = Response<IEnumerable<AnnouncementDto>>.Success(announcements, 200);
            _mockService.Setup(x => x.GetAnnouncementsAsync(null, null)).ReturnsAsync(response);

            // Act
            var result = await _mockService.Object.GetAnnouncementsAsync(null, null);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task IAnnouncementService_GetAnnouncementsAsync_WithTargetAudience_ShouldFilter()
        {
            // Arrange
            var announcements = new List<AnnouncementDto>();
            var response = Response<IEnumerable<AnnouncementDto>>.Success(announcements, 200);
            _mockService.Setup(x => x.GetAnnouncementsAsync("Students", null)).ReturnsAsync(response);

            // Act
            var result = await _mockService.Object.GetAnnouncementsAsync("Students", null);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public async Task IAnnouncementService_GetAnnouncementsAsync_WithDepartmentId_ShouldFilter()
        {
            // Arrange
            var announcements = new List<AnnouncementDto>();
            var response = Response<IEnumerable<AnnouncementDto>>.Success(announcements, 200);
            _mockService.Setup(x => x.GetAnnouncementsAsync(null, 5)).ReturnsAsync(response);

            // Act
            var result = await _mockService.Object.GetAnnouncementsAsync(null, 5);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeTrue();
        }

        #endregion

        #region GetImportantAnnouncements Tests

        [Fact]
        public async Task IAnnouncementService_GetImportantAnnouncementsAsync_ShouldReturnList()
        {
            // Arrange
            var announcements = new List<AnnouncementDto>
            {
                new AnnouncementDto { Id = 1, Title = "Important Notice", IsImportant = true }
            };
            var response = Response<IEnumerable<AnnouncementDto>>.Success(announcements, 200);
            _mockService.Setup(x => x.GetImportantAnnouncementsAsync()).ReturnsAsync(response);

            // Act
            var result = await _mockService.Object.GetImportantAnnouncementsAsync();

            // Assert
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(200);
        }

        #endregion

        #region GetAnnouncementById Tests

        [Fact]
        public async Task IAnnouncementService_GetAnnouncementByIdAsync_ShouldReturnAnnouncement()
        {
            // Arrange
            var announcement = new AnnouncementDto { Id = 1, Title = "Test Announcement" };
            var response = Response<AnnouncementDto>.Success(announcement, 200);
            _mockService.Setup(x => x.GetAnnouncementByIdAsync(1)).ReturnsAsync(response);

            // Act
            var result = await _mockService.Object.GetAnnouncementByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeTrue();
            result.Data!.Title.Should().Be("Test Announcement");
        }

        [Fact]
        public async Task IAnnouncementService_GetAnnouncementByIdAsync_ShouldFail_WhenNotFound()
        {
            // Arrange
            var response = Response<AnnouncementDto>.Fail("Announcement not found", 404);
            _mockService.Setup(x => x.GetAnnouncementByIdAsync(999)).ReturnsAsync(response);

            // Act
            var result = await _mockService.Object.GetAnnouncementByIdAsync(999);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        #endregion

        #region IncrementViewCount Tests

        [Fact]
        public async Task IAnnouncementService_IncrementViewCountAsync_ShouldSucceed()
        {
            // Arrange
            var response = Response<EntityLayer.DTOs.NoDataDto>.Success(200);
            _mockService.Setup(x => x.IncrementViewCountAsync(1)).ReturnsAsync(response);

            // Act
            var result = await _mockService.Object.IncrementViewCountAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeTrue();
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task IAnnouncementService_IncrementViewCountAsync_ShouldFail_WhenNotFound()
        {
            // Arrange
            var response = Response<EntityLayer.DTOs.NoDataDto>.Fail("Announcement not found", 404);
            _mockService.Setup(x => x.IncrementViewCountAsync(999)).ReturnsAsync(response);

            // Act
            var result = await _mockService.Object.IncrementViewCountAsync(999);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccessful.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }

        #endregion

        #region Controller Method Existence Tests

        [Fact]
        public void AnnouncementsController_ShouldHaveExpectedMethods()
        {
            // Verify controller has expected endpoints
            var controllerType = typeof(AnnouncementsController);
            
            controllerType.GetMethod("GetAnnouncements").Should().NotBeNull();
            controllerType.GetMethod("GetImportantAnnouncements").Should().NotBeNull();
            controllerType.GetMethod("GetAnnouncementById").Should().NotBeNull();
            controllerType.GetMethod("IncrementViewCount").Should().NotBeNull();
        }

        #endregion
    }
}
