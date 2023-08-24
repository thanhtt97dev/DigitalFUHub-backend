using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RealTimeServerAPI.DTOs;
using RealTimeServerAPI.Hubs;

namespace RealTimeServerAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class NotificationsController : ControllerBase
	{
		private readonly IHubContext<NotificationHub> _hubContext;

		public NotificationsController(IHubContext<NotificationHub> hubContext)
		{
			_hubContext = hubContext;
		}

		[HttpPost("sendNotification")]
		public async Task<IActionResult> SendNotification([FromBody] NotificationRequestDTO request)
		{
			try
			{
				await _hubContext.Clients.Client(request.ConnectionId).SendAsync("ReceiveNotification", request.Message);
				return Ok();
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
	}
}
