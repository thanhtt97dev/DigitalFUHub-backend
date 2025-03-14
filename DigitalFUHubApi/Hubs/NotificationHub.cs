﻿using AutoMapper;
using DataAccess.IRepositories;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using DigitalFUHubApi.Services;
using DigitalFUHubApi.Comons;
using DTOs.Notification;
using Comons;
using System;
using DigitalFUHubApi.Managers.IRepositories;

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

		public override Task OnConnectedAsync()
		{
			var userId = hubService.GetUserIdFromHubCaller(Context);
			var connectionId = hubService.GetConnectionIdFromHubCaller(Context);
			connectionManager.AddConnection(userId, Constants.SIGNAL_R_NOTIFICATION_HUB, connectionId);
			return base.OnConnectedAsync();
		}

		public override Task OnDisconnectedAsync(Exception? exception)
		{
			var userId = hubService.GetUserIdFromHubCaller(Context);
			var connectionId = hubService.GetConnectionIdFromHubCaller(Context);
			connectionManager.RemoveConnection(userId, Constants.SIGNAL_R_NOTIFICATION_HUB, connectionId);
			return base.OnDisconnectedAsync(exception);
		}

    }
}
