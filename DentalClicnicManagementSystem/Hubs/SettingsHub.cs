using Microsoft.AspNetCore.SignalR;

namespace CMS.Hubs
{
    public class SettingsHub : Hub
    {
        public async Task JoinSettingsGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "SettingsGroup");
        }

        public async Task LeaveSettingsGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SettingsGroup");
        }
    }
}
