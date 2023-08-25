using AutoMapper;
using BusinessObject;
using DataAccess.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using RealTimeServerAPI.DTOs;
using RealTimeServerAPI.Hubs;
using RealTimeServerAPI.Managers;

namespace RealTimeServerAPI.Controllers
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
		public async Task<IActionResult> SendNotification([FromRoute]int userId, NotificationRequest notificationRequest)
		{
			HashSet<string>? connections = _connectionManager.GetConnections(userId);
			try
			{
				Notification notification = new Notification()
				{
					UserId = userId,
					Title = notificationRequest.Title,
					Content = notificationRequest.Content,
					Link = "",
					DateCreated = DateTime.Now,
					IsReaded = false,
				};

				if (connections == null || connections.Count == 0) return Ok("User not online!");
				foreach (var connection in connections)
				{
					await _hubContext.Clients.Clients(connection).SendAsync("ReceiveNotification",
						JsonConvert.SerializeObject(_mapper.Map<NotificationRespone>(notification)));
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
