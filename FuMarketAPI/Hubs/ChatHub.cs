using FuMarketAPI.Services;
using Microsoft.AspNetCore.SignalR;

namespace FuMarketAPI.Hubs
{
    public class ChatHub : Hub
    {
        private readonly HubConnectionService _hubConnectionService;

        public ChatHub(HubConnectionService hubConnectionService) {
            _hubConnectionService = hubConnectionService;

        }

        public override Task OnConnectedAsync()
        {
            _hubConnectionService.AddConnection(Context);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _hubConnectionService.RemoveConnection(Context);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
