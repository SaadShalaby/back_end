using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace MedicalApp.API.Hubs
{
    /// <summary>
    /// Real-time chat hub for typing indicators.
    /// Clients join conversation groups and broadcast typing events.
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        // ─────────────────────────────────────────────
        // Join a conversation group
        // ─────────────────────────────────────────────
        public async Task JoinConversation(string conversationId)
        {
            if (string.IsNullOrWhiteSpace(conversationId)) return;
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        }

        // ─────────────────────────────────────────────
        // Leave a conversation group
        // ─────────────────────────────────────────────
        public async Task LeaveConversation(string conversationId)
        {
            if (string.IsNullOrWhiteSpace(conversationId)) return;
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
        }

        // ─────────────────────────────────────────────
        // Broadcast: user started typing
        // ─────────────────────────────────────────────
        public async Task StartTyping(string conversationId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(conversationId)) return;

            // Notify everyone in the group EXCEPT the sender
            await Clients.OthersInGroup(conversationId)
                         .SendAsync("UserIsTyping", userId);
        }

        // ─────────────────────────────────────────────
        // Broadcast: user stopped typing
        // ─────────────────────────────────────────────
        public async Task StopTyping(string conversationId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(conversationId)) return;

            await Clients.OthersInGroup(conversationId)
                         .SendAsync("UserStoppedTyping", userId);
        }
    }
}
