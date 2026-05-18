using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MedicalApp.API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        /// <summary>
        /// عند اتصال أي يوزر، بيدخل في Group خاصة به عشان نقدر نبعتله إشعار بـ userId بتاعه
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
