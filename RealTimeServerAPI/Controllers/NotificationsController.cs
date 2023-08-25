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

		public NotificationsController(IHubContext<NotificationHub> hubContext, IConnectionManager connectionManager)
		{
			_hubContext = hubContext;
			_connectionManager = connectionManager;	
		}

		#region Send notification to a user
		[HttpPost("sendNotification/{userId}")]
		public async Task<IActionResult> SendNotification([FromRoute]string userId, NotificationRequest notificationRequest)
		{
			HashSet<string>? connections = _connectionManager.GetConnections(userId);
			try
			{
				if (connections == null || connections.Count == 0) return Conflict();
				foreach (var connection in connections) 
				{
					var notificationRespone = new NotificationRespone(notificationRequest.Title, notificationRequest.Message);
					await _hubContext.Clients.Clients(connection).SendAsync("ReceiveNotification",
						JsonConvert.SerializeObject(notificationRespone));	
				}
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
