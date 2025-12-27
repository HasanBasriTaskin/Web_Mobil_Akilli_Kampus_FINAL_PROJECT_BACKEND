using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SMARTCAMPUS.API.Controllers;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Notifications;
using SMARTCAMPUS.EntityLayer.Enums;
using System.Security.Claims;
using Xunit;

namespace SMARTCAMPUS.Tests.Controllers
{
    public class NotificationsControllerTests
    {
        private readonly Mock<IAdvancedNotificationService> _mockNotificationService;
        private readonly NotificationsController _controller;

        public NotificationsControllerTests()
        {
            _mockNotificationService = new Mock<IAdvancedNotificationService>();
            _controller = new NotificationsController(_mockNotificationService.Object);
            SetupHttpContext("user1");
        }

        private void SetupHttpContext(string userId)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task GetNotifications_ShouldReturnOk()
        {
            _mockNotificationService.Setup(x => x.GetUserNotificationsAsync("user1", 1, 20))
                .ReturnsAsync(Response<List<NotificationDto>>.Success(new List<NotificationDto>(), 200));

            var result = await _controller.GetNotifications();

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetUnreadCount_ShouldReturnOk()
        {
            _mockNotificationService.Setup(x => x.GetUnreadCountAsync("user1"))
                .ReturnsAsync(Response<UnreadCountDto>.Success(new UnreadCountDto { Count = 5 }, 200));

            var result = await _controller.GetUnreadCount();

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task MarkAsRead_ShouldReturnOk()
        {
            _mockNotificationService.Setup(x => x.MarkAsReadAsync(1, "user1"))
                .ReturnsAsync(Response<NoDataDto>.Success(200));

            var result = await _controller.MarkAsRead(1);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task MarkAllAsRead_ShouldReturnOk()
        {
            _mockNotificationService.Setup(x => x.MarkAllAsReadAsync("user1"))
                .ReturnsAsync(Response<NoDataDto>.Success(200));

            var result = await _controller.MarkAllAsRead();

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetPreferences_ShouldReturnOk()
        {
            _mockNotificationService.Setup(x => x.GetPreferencesAsync("user1"))
                .ReturnsAsync(Response<List<NotificationPreferenceDto>>.Success(new List<NotificationPreferenceDto>(), 200));

            var result = await _controller.GetPreferences();

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task UpdatePreferences_ShouldReturnOk()
        {
            var dto = new UpdatePreferencesDto { Preferences = new List<PreferenceItem>() };
            _mockNotificationService.Setup(x => x.UpdatePreferencesAsync("user1", dto))
                .ReturnsAsync(Response<NoDataDto>.Success(200));

            var result = await _controller.UpdatePreferences(dto);

            result.Should().BeOfType<ObjectResult>();
            ((ObjectResult)result).StatusCode.Should().Be(200);
        }
    }
}

