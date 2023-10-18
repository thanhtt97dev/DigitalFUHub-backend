using Microsoft.AspNetCore.SignalR;
using DigitalFUHubApi.Managers;

namespace DigitalFUHubApi.Services
{
	public class HubConnectionService
	{
		private IConnectionManager connectionManager;
		public HubConnectionService(IConnectionManager connectionManager)
		{
			this.connectionManager = connectionManager;
		}

		public void AddConnection(HubCallerContext hubCallerContext, string hubName)
		{
			var httpContext = hubCallerContext.GetHttpContext();
			if (httpContext == null) return;

			var userIdRaw = httpContext.Request.Query["userId"];
			if (string.IsNullOrEmpty(userIdRaw)) return;

			int userId;
			int.TryParse(userIdRaw, out userId);
			connectionManager.AddConnection(userId,hubName, hubCallerContext.ConnectionId);
		}

		public void RemoveConnection(HubCallerContext hubCallerContext, string hubName)
		{
			var connectionId = hubCallerContext.ConnectionId;
			connectionManager.RemoveConnection(connectionId, hubName);
		}

		public bool CheckUserConnected(long userId) 
		{
			return connectionManager.CheckUserConnectd(userId);	
		}

		public int GetUserIdFromHubCaller(HubCallerContext hubCallerContext)
		{
			var httpContext = hubCallerContext.GetHttpContext();
			if (httpContext == null) return 0;

			var userIdRaw = httpContext.Request.Query["userId"];
			if (string.IsNullOrEmpty(userIdRaw)) return 0;

			int userId;
			int.TryParse(userIdRaw, out userId);

			return userId;
		}


	}
}
