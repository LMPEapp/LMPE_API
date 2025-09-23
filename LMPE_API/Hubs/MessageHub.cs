using LMPE_API.Models;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace LMPE_API.Hubs
{
    public class MessageHub : Hub
    {
        // Rejoindre un groupe SignalR côté client
        public async Task JoinGroup(long groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"group_{groupId}");
        }

        public async Task LeaveGroup(long groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group_{groupId}");
        }
        // Typing indicator
        public async Task Typing(long groupId, long userId)
        {
            // On notifie tous les autres clients sauf celui qui tape
            await Clients.GroupExcept($"group_{groupId}", Context.ConnectionId)
                         .SendAsync("UserTyping", userId);
        }
    }
}
