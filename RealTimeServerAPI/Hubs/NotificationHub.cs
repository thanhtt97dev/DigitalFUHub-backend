using Microsoft.AspNetCore.SignalR;
using RealTimeServerAPI.Managers;

namespace RealTimeServerAPI.Hubs
{
	public class NotificationHub: Hub
	{
		private IConnectionManager _connectionManager;	

		public NotificationHub(IConnectionManager connectionManager)
		{
			_connectionManager = connectionManager;	
		}

		public override Task OnConnectedAsync()
		{
			AddConnection();
			return base.OnConnectedAsync();
		}

		public override Task OnDisconnectedAsync(Exception? exception)
		{
			RemoveConnection();
			return base.OnDisconnectedAsync(exception);
		}

		public void AddConnection()
		{
			var httpContext = this.Context.GetHttpContext();
			if (httpContext == null) return;
			var userId = httpContext.Request.Query["userId"];
			if (string.IsNullOrEmpty(userId)) return;		
			_connectionManager.AddConnection(userId, Context.ConnectionId);
		}

		public void RemoveConnection() 
		{
			var connectionId = Context.ConnectionId;
			_connectionManager.RemoveConnection(connectionId);
		}


	}
}
