using DigitalFUHubApi.Services;
using Microsoft.AspNetCore.SignalR;
using Comons;
using Microsoft.AspNetCore.Authorization;
using DigitalFUHubApi.Managers.IRepositories;

namespace DigitalFUHubApi.Hubs
{
    [Authorize]
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
			connectionManager.AddConnection(userId, Constants.SIGNAL_R_CHAT_HUB, connectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
			var userId = hubService.GetUserIdFromHubCaller(Context);
			var connectionId = hubService.GetConnectionIdFromHubCaller(Context);
			connectionManager.RemoveConnection(userId, Constants.SIGNAL_R_CHAT_HUB, connectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
