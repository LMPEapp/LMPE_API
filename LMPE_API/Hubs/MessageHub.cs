using LMPE_API.Models;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace LMPE_API.Hubs
{
    public class MessageHub : Hub
    {
        public static string UserTyping = "UserTyping";
        public static string ReceiveMessage = "ReceiveMessage";
        public static string UpdateMessage = "UpdateMessage";
        public static string DeleteMessage = "DeleteMessage";

        public static string Groupe = "group_";
        // Rejoindre un groupe SignalR côté client
        public async Task JoinGroup(long groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{MessageHub.Groupe}{groupId}");
        }

        public async Task LeaveGroup(long groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{MessageHub.Groupe}{groupId}");
        }
        // Typing indicator
        public async Task Typing(long groupId, User user)
        {
            // On notifie tous les autres clients sauf celui qui tape
            await Clients.GroupExcept($"{MessageHub.Groupe}{groupId}", Context.ConnectionId)
                         .SendAsync(MessageHub.UserTyping, user);
        }
    }
}
