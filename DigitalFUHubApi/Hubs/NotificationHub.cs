using AutoMapper;
using DataAccess.IRepositories;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using DigitalFUHubApi.Managers;
using Microsoft.AspNetCore.Authorization;
using DigitalFUHubApi.Services;
using DigitalFUHubApi.Comons;
using DTOs.Notification;
using Comons;

namespace DigitalFUHubApi.Hubs
{
	[Authorize]
    public class NotificationHub : Hub
	{

		private readonly INotificationRepositiory notificationRepositiory;
		private readonly IMapper mapper;
		private readonly HubService hubService;
		private readonly IConnectionManager connectionManager;

		public NotificationHub(INotificationRepositiory notificationRepositiory, IMapper mapper, HubService hubService, IConnectionManager connectionManager)
		{
			this.notificationRepositiory = notificationRepositiory;
			this.mapper = mapper;
			this.hubService = hubService;
			this.connectionManager = connectionManager;
		}

		public override async Task OnConnectedAsync()
		{
			var userId = hubService.GetUserIdFromHubCaller(Context);
			var connectionId = hubService.GetConnectionIdFromHubCaller(Context);
			connectionManager.AddConnection(userId, Constants.SIGNAL_R_NOTIFICATION_HUB, connectionId);
			await SendAllNotificationToUserCaller();
		}

		public override Task OnDisconnectedAsync(Exception? exception)
		{
			var userId = hubService.GetUserIdFromHubCaller(Context);
			var connectionId = hubService.GetConnectionIdFromHubCaller(Context);
			connectionManager.RemoveConnection(userId, Constants.SIGNAL_R_NOTIFICATION_HUB, connectionId);
			return base.OnDisconnectedAsync(exception);
		}

		private async Task SendAllNotificationToUserCaller()
		{
			var userId = hubService.GetUserIdFromHubCaller(Context);
			if (userId == 0) return;

			var notifications = notificationRepositiory.GetNotifications(userId);
			await Clients.Caller.SendAsync(Constants.SIGNAL_R_NOTIFICATION_HUB_RECEIVE_ALL_NOTIFICATION,
						JsonConvert.SerializeObject(mapper.Map<List<NotificationRespone>>(notifications)));
		}

    }
}
