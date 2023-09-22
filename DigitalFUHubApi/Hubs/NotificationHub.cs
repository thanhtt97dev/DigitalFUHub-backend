using AutoMapper;
using DataAccess.IRepositories;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using DigitalFUHubApi.Managers;
using Microsoft.AspNetCore.Authorization;
using DigitalFUHubApi.Services;
using DTOs.Notification;

namespace DigitalFUHubApi.Hubs
{

    public class NotificationHub : Hub
	{

		private readonly INotificationRepositiory _notificationRepositiory;

		private readonly IMapper _mapper;

		private readonly HubConnectionService _hubConnectionService;

		public NotificationHub( INotificationRepositiory notificationRepositiory, IMapper mapper, 
			HubConnectionService hubConnectionService)
		{
			_notificationRepositiory = notificationRepositiory;
			_mapper = mapper;
			_hubConnectionService = hubConnectionService;
		}

		public override async Task OnConnectedAsync()
		{
			_hubConnectionService.AddConnection(Context);
			await SendAllNotificationToUserCaller();
		}

		public override Task OnDisconnectedAsync(Exception? exception)
		{
			_hubConnectionService.RemoveConnection(Context);
			return base.OnDisconnectedAsync(exception);
		}

		private async Task SendAllNotificationToUserCaller()
		{
			var userId = _hubConnectionService.GetUserIdFromHubCaller(Context);
			if (userId == 0) return;

			var notifications = _notificationRepositiory.GetNotifications(userId);
			await Clients.Caller.SendAsync("ReceiveAllNotification",
						JsonConvert.SerializeObject(_mapper.Map<List<NotificationRespone>>(notifications)));
		}

        /*

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
		*/
    }
}
