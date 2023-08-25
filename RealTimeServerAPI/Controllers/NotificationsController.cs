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

		[HttpPost("sendNotification/{userId}")]
		public async Task<IActionResult> SendNotification(string userId, NotificationRespone notificationRespone)
		{
			HashSet<string>? connections = _connectionManager.GetConnections(userId);
			try
			{
				if (connections == null || connections.Count == 0) return Conflict();
				foreach (var connection in connections) 
				{
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
	}
}
