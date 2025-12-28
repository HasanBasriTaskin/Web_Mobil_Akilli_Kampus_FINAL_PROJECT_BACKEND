using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;
using SMARTCAMPUS.API.Hubs;
using System.Security.Claims;
using Xunit;

namespace SMARTCAMPUS.Tests.Hubs
{
    public class NotificationHubTests
    {

        [Fact]
        public async Task JoinUserGroup_ShouldAddUserToGroup_WhenUserIdExists()
        {
            var mockGroups = new Mock<IGroupManager>();
            var mockContext = new Mock<HubCallerContext>();
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "user1") };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            mockContext.Setup(x => x.User).Returns(principal);
            mockContext.Setup(x => x.ConnectionId).Returns("conn1");
            
            var hub = new NotificationHub();
            hub.Context = mockContext.Object;
            hub.Groups = mockGroups.Object;
            
            await hub.JoinUserGroup();
            
            mockGroups.Verify(x => x.AddToGroupAsync("conn1", "user_user1", default), Times.Once);
        }

        [Fact]
        public async Task JoinUserGroup_ShouldNotAddToGroup_WhenUserIdIsNull()
        {
            var mockGroups = new Mock<IGroupManager>();
            var mockContext = new Mock<HubCallerContext>();
            var principal = new ClaimsPrincipal(new ClaimsIdentity());
            
            mockContext.Setup(x => x.User).Returns(principal);
            mockContext.Setup(x => x.ConnectionId).Returns("conn1");
            
            var hub = new NotificationHub();
            hub.Context = mockContext.Object;
            hub.Groups = mockGroups.Object;
            
            await hub.JoinUserGroup();
            
            mockGroups.Verify(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
        }

        [Fact]
        public async Task JoinRoleGroup_ShouldAddUserToRoleGroup()
        {
            var mockGroups = new Mock<IGroupManager>();
            var mockContext = new Mock<HubCallerContext>();
            
            mockContext.Setup(x => x.ConnectionId).Returns("conn1");
            
            var hub = new NotificationHub();
            hub.Context = mockContext.Object;
            hub.Groups = mockGroups.Object;
            
            await hub.JoinRoleGroup("Admin");
            
            mockGroups.Verify(x => x.AddToGroupAsync("conn1", "role_Admin", default), Times.Once);
        }

        [Fact]
        public async Task LeaveGroup_ShouldRemoveUserFromGroup()
        {
            var mockGroups = new Mock<IGroupManager>();
            var mockContext = new Mock<HubCallerContext>();
            
            mockContext.Setup(x => x.ConnectionId).Returns("conn1");
            
            var hub = new NotificationHub();
            hub.Context = mockContext.Object;
            hub.Groups = mockGroups.Object;
            
            await hub.LeaveGroup("user_user1");
            
            mockGroups.Verify(x => x.RemoveFromGroupAsync("conn1", "user_user1", default), Times.Once);
        }

        [Fact]
        public async Task OnConnectedAsync_ShouldAddUserToUserGroup_WhenUserIdExists()
        {
            var mockGroups = new Mock<IGroupManager>();
            var mockContext = new Mock<HubCallerContext>();
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "user1") };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            mockContext.Setup(x => x.User).Returns(principal);
            mockContext.Setup(x => x.ConnectionId).Returns("conn1");
            
            var hub = new NotificationHub();
            hub.Context = mockContext.Object;
            hub.Groups = mockGroups.Object;
            
            await hub.OnConnectedAsync();
            
            mockGroups.Verify(x => x.AddToGroupAsync("conn1", "user_user1", default), Times.Once);
        }

        [Fact]
        public async Task OnConnectedAsync_ShouldAddUserToRoleGroups_WhenRolesExist()
        {
            var mockGroups = new Mock<IGroupManager>();
            var mockContext = new Mock<HubCallerContext>();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "user1"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.Role, "Faculty")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            
            mockContext.Setup(x => x.User).Returns(principal);
            mockContext.Setup(x => x.ConnectionId).Returns("conn1");
            
            var hub = new NotificationHub();
            hub.Context = mockContext.Object;
            hub.Groups = mockGroups.Object;
            
            await hub.OnConnectedAsync();
            
            mockGroups.Verify(x => x.AddToGroupAsync("conn1", "user_user1", default), Times.Once);
            mockGroups.Verify(x => x.AddToGroupAsync("conn1", "role_Admin", default), Times.Once);
            mockGroups.Verify(x => x.AddToGroupAsync("conn1", "role_Faculty", default), Times.Once);
        }

        [Fact]
        public async Task OnConnectedAsync_ShouldNotAddToGroups_WhenUserIdIsNull()
        {
            var mockGroups = new Mock<IGroupManager>();
            var mockContext = new Mock<HubCallerContext>();
            var principal = new ClaimsPrincipal(new ClaimsIdentity());
            
            mockContext.Setup(x => x.User).Returns(principal);
            mockContext.Setup(x => x.ConnectionId).Returns("conn1");
            
            var hub = new NotificationHub();
            hub.Context = mockContext.Object;
            hub.Groups = mockGroups.Object;
            
            await hub.OnConnectedAsync();
            
            mockGroups.Verify(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
        }

        [Fact]
        public async Task OnDisconnectedAsync_ShouldCallBase()
        {
            var mockContext = new Mock<HubCallerContext>();
            mockContext.Setup(x => x.ConnectionId).Returns("conn1");
            
            var hub = new NotificationHub();
            hub.Context = mockContext.Object;
            
            await hub.OnDisconnectedAsync(null);
            
            // Base method should be called (no exception means success)
        }
    }
}
