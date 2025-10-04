using LMPE_API.Models;
using Microsoft.AspNetCore.SignalR;

namespace LMPE_API.Hubs
{
    public class CourbecaHub : Hub
    {
        public static string CourbecaCreated = "CourbecaCreated";
        public static string CourbecaDeleted = "CourbecaDeleted";

        public static string Groupe = "CourbecaHub";

        // Rejoindre un groupe SignalR pour un agenda précis
        public async Task JoinCourbeca()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{Groupe}");
        }

        public async Task LeaveCourbeca()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{Groupe}");
        }
    }
}
