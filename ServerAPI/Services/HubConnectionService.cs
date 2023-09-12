using Microsoft.AspNetCore.SignalR;
using ServerAPI.Managers;

namespace ServerAPI.Services
{
	public class HubConnectionService
	{
		private IConnectionManager _connectionManager;
		public HubConnectionService(IConnectionManager connectionManager)
		{
			_connectionManager = connectionManager;
		}

		public void AddConnection(HubCallerContext hubCallerContext)
		{
			var httpContext = hubCallerContext.GetHttpContext();
			if (httpContext == null) return;

			var userIdRaw = httpContext.Request.Query["userId"];
			if (string.IsNullOrEmpty(userIdRaw)) return;

			int userId;
			int.TryParse(userIdRaw, out userId);
			_connectionManager.AddConnection(userId, hubCallerContext.ConnectionId);
		}

		public void RemoveConnection(HubCallerContext hubCallerContext)
		{
			var connectionId = hubCallerContext.ConnectionId;
			_connectionManager.RemoveConnection(connectionId);
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
