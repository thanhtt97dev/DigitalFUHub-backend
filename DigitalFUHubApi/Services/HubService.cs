using AutoMapper;
using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DigitalFUHubApi.Hubs;
using DigitalFUHubApi.Managers.IRepositories;
using DTOs.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Quartz.Util;

namespace DigitalFUHubApi.Services
{
    public class HubService
	{

		private readonly IConnectionManager connectionManager;
		private readonly IMapper mapper;
		private readonly IHubContext<NotificationHub> notificationHub;
		private readonly INotificationRepositiory notificationRepositiory;

		public HubService(IConnectionManager connectionManager, IMapper mapper, IHubContext<NotificationHub> notificationHub, INotificationRepositiory notificationRepositiory)
		{
			this.connectionManager = connectionManager;
			this.mapper = mapper;
			this.notificationHub = notificationHub;
			this.notificationRepositiory = notificationRepositiory;
		}

		public string GetConnectionIdFromHubCaller(HubCallerContext hubCallerContext)
		{
			var connectionId = hubCallerContext.ConnectionId;
			return connectionId;
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

        public int GetVisibleNotificationsFromHubCaller(HubCallerContext hubCallerContext)
        {
            var httpContext = hubCallerContext.GetHttpContext();
            if (httpContext == null) return 0;

            var visibleNotificationsRaw = httpContext.Request.Query["visibleNotifications"];
            if (string.IsNullOrEmpty(visibleNotificationsRaw)) return 0;

            int visibleNotifications;
            int.TryParse(visibleNotificationsRaw, out visibleNotifications);

            return visibleNotifications;
        }

        public async Task SendNotification(long userId, string title, string content, string link)
		{
			HashSet<string>? connections = connectionManager
				.GetConnections(userId, Constants.SIGNAL_R_NOTIFICATION_HUB);

			Notification notification = new Notification()
			{
				UserId = userId,
				Title = title,
				Content = content,
				Link = link,
				DateCreated = DateTime.Now,
				IsReaded = false,
			};

			notificationRepositiory.AddNotification(notification);

			if (connections != null)
			{
				foreach (var connection in connections)
				{
					await notificationHub.Clients.Clients(connection)
						.SendAsync(Constants.SIGNAL_R_NOTIFICATION_HUB_RECEIVE_NOTIFICATION,
						JsonConvert.SerializeObject(mapper.Map<NotificationDetailResponeDTO>(notification)));
				}
			}
		}

	}
}
