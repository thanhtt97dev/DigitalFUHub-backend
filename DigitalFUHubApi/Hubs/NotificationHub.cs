using AutoMapper;
using DataAccess.IRepositories;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using DigitalFUHubApi.Managers;
using Microsoft.AspNetCore.Authorization;
using DigitalFUHubApi.Services;
using DigitalFUHubApi.Comons;
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
			_hubConnectionService.AddConnection(Context, Constants.SIGNAL_R_NOTIFICATION_HUB);
			await SendAllNotificationToUserCaller();
		}

		public override Task OnDisconnectedAsync(Exception? exception)
		{
			_hubConnectionService.RemoveConnection(Context, Constants.SIGNAL_R_NOTIFICATION_HUB);
			return base.OnDisconnectedAsync(exception);
		}

		private async Task SendAllNotificationToUserCaller()
		{
			var userId = _hubConnectionService.GetUserIdFromHubCaller(Context);
			if (userId == 0) return;

			var notifications = _notificationRepositiory.GetNotifications(userId);
			await Clients.Caller.SendAsync(Constants.SIGNAL_R_CHAT_HUB_RECEIVE_ALL_NOTIFICATION,
						JsonConvert.SerializeObject(_mapper.Map<List<NotificationRespone>>(notifications)));
		}

    }
}
