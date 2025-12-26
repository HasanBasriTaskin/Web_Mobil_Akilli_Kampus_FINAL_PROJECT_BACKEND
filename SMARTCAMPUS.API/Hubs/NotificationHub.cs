using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SMARTCAMPUS.API.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time notifications
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        /// <summary>
        /// Kullanıcıyı kendi grubuna ekler (User ID bazlı)
        /// </summary>
        public async Task JoinUserGroup()
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
        }

        /// <summary>
        /// Kullanıcıyı rol grubuna ekler
        /// </summary>
        public async Task JoinRoleGroup(string role)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"role_{role}");
        }

        /// <summary>
        /// Kullanıcıyı gruptan çıkarır
        /// </summary>
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        /// <summary>
        /// Bağlantı kurulduğunda çağrılır
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                
                // Kullanıcının rollerini gruplara ekle
                var roles = Context.User?.FindAll(System.Security.Claims.ClaimTypes.Role);
                if (roles != null)
                {
                    foreach (var role in roles)
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, $"role_{role.Value}");
                    }
                }
            }
            
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Bağlantı kesildiğinde çağrılır
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
