using DigitalFUHubApi.Services;
using Microsoft.AspNetCore.SignalR;
using Comons;

namespace DigitalFUHubApi.Hubs
{
    public class ChatHub : Hub
    {
        private readonly HubConnectionService _hubConnectionService;

        public ChatHub(HubConnectionService hubConnectionService) {
            _hubConnectionService = hubConnectionService;

        }

        public override Task OnConnectedAsync()
        {
            _hubConnectionService.AddConnection(Context, Constants.SIGNAL_R_CHAT_HUB);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _hubConnectionService.RemoveConnection(Context, Constants.SIGNAL_R_CHAT_HUB);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
