using DigitalFUHubApi.Services;
using Microsoft.AspNetCore.SignalR;
using Comons;
using DigitalFUHubApi.Managers;

namespace DigitalFUHubApi.Hubs
{
    public class ChatHub : Hub
    {
        private readonly HubService hubService;
        private readonly IConnectionManager connectionManager;

		public ChatHub(HubService hubService, IConnectionManager connectionManager)
		{
			this.hubService = hubService;
			this.connectionManager = connectionManager;
		}

		public override Task OnConnectedAsync()
        {
			var userId = hubService.GetUserIdFromHubCaller(Context);
			var connectionId = hubService.GetConnectionIdFromHubCaller(Context);
			connectionManager.AddConnection(userId, connectionId, Constants.SIGNAL_R_CHAT_HUB);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
			var userId = hubService.GetUserIdFromHubCaller(Context);
			var connectionId = hubService.GetConnectionIdFromHubCaller(Context);
			connectionManager.RemoveConnection(userId, connectionId, Constants.SIGNAL_R_CHAT_HUB);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
