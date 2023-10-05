using AutoMapper;
using DataAccess.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using DigitalFUHubApi.Hubs;
using DigitalFUHubApi.Managers;
using DigitalFUHubApi.Comons;
using BusinessObject.Entities;
using DTOs.Notification;
using Comons;

namespace DigitalFUHubApi.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
	public class NotificationsController : ControllerBase
	{
		private readonly IHubContext<NotificationHub> _hubContext;

		private readonly IConnectionManager _connectionManager;

		private readonly INotificationRepositiory _notificationRepositiory;

		private readonly IMapper _mapper;

		public NotificationsController(IHubContext<NotificationHub> hubContext, IConnectionManager connectionManager,
			INotificationRepositiory notificationRepositiory, IMapper mapper)
		{
			_hubContext = hubContext;
			_connectionManager = connectionManager;
			_notificationRepositiory = notificationRepositiory;
			_mapper = mapper;
		}

		#region Send notification to a user
		[Authorize]
		[HttpPost("sendNotification/{userId}")]
		public async Task<IActionResult> SendNotification([FromRoute] int userId, NotificationRequest notificationRequest)
		{
			try
			{
				HashSet<string>? connections = _connectionManager
					.GetConnections(userId, Constants.SIGNAL_R_NOTIFICATION_HUB);

				Notification notification = new Notification()
				{
					UserId = userId,
					Title = notificationRequest.Title,
					Content = notificationRequest.Content,
					Link = "",
					DateCreated = DateTime.Now,
					IsReaded = false,
				};

				if (connections != null)
				{
					foreach (var connection in connections)
					{
						await _hubContext.Clients.Clients(connection)
							.SendAsync(Constants.SIGNAL_R_NOTIFICATION_HUB_RECEIVE_NOTIFICATION,
							JsonConvert.SerializeObject(_mapper.Map<NotificationRespone>(notification)));
					}
				}
				
				_notificationRepositiory.AddNotification(notification);
				return Ok();
			}
			catch
			{
				return StatusCode(500);
			}
		}
		#endregion
	}
}
