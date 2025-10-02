using LMPE_API.Models;
using Microsoft.AspNetCore.SignalR;

namespace LMPE_API.Hubs
{
    public class AgendaHub : Hub
    {
        public static string AgendaCreated = "AgendaCreated";
        public static string AgendaUpdated = "AgendaUpdated";
        public static string AgendaDeleted = "AgendaDeleted";
        public static string AgendaUsersUpdated = "AgendaUsersUpdated";

        public static string Groupe = "Agenda";

        // Rejoindre un groupe SignalR pour un agenda précis
        public async Task JoinAgendasGlobal()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{Groupe}");
        }

        public async Task LeaveAgendasGlobal()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{Groupe}");
        }
        public async Task JoinAgenda(long agendaId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{Groupe}{agendaId}");
        }

        public async Task LeaveAgenda(long agendaId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{Groupe}{agendaId}");
        }
    }
}
