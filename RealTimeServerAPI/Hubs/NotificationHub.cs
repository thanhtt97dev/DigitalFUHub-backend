using AutoMapper;
using BusinessObject;
using DataAccess.IRepositories;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using RealTimeServerAPI.DTOs;
using RealTimeServerAPI.Managers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace RealTimeServerAPI.Hubs
{
	public class NotificationHub: Hub
	{
		private IConnectionManager _connectionManager;

		private readonly INotificationRepositiory _notificationRepositiory;

		private readonly IMapper _mapper;

		public NotificationHub(IConnectionManager connectionManager, INotificationRepositiory notificationRepositiory,
			IMapper mapper)
		{
			_connectionManager = connectionManager;
			_notificationRepositiory = notificationRepositiory;
			_mapper = mapper;	
		}

		public override async Task OnConnectedAsync()
		{
			AddConnection();

		 	await SendAllNotificationToUserCaller();
		}

		public override Task OnDisconnectedAsync(Exception? exception)
		{
			RemoveConnection();
			return base.OnDisconnectedAsync(exception);
		}

		private async Task SendAllNotificationToUserCaller()
		{
			var httpContext = this.Context.GetHttpContext();
			if (httpContext == null) return;

			var userIdRaw = httpContext.Request.Query["userId"];
			if (string.IsNullOrEmpty(userIdRaw)) return;

			int userId;
			int.TryParse(userIdRaw, out userId);

			var notifications = _notificationRepositiory.GetNotifications(userId);

			await Clients.Caller.SendAsync("ReceiveAllNotification",
						JsonConvert.SerializeObject(_mapper.Map<List<NotificationRespone>>(notifications)));
		}

		public void AddConnection()
		{
			var httpContext = this.Context.GetHttpContext();
			if (httpContext == null) return;

			var userIdRaw = httpContext.Request.Query["userId"];
			if (string.IsNullOrEmpty(userIdRaw)) return;

			int userId;
			int.TryParse(userIdRaw, out userId);
			_connectionManager.AddConnection(userId, Context.ConnectionId);
		}

		public void RemoveConnection() 
		{
			var connectionId = Context.ConnectionId;
			_connectionManager.RemoveConnection(connectionId);
		}


	}
}
