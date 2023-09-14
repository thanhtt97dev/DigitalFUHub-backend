using BusinessObject;
using DTOs;
using FuMarketAPI.Hubs;
using FuMarketAPI.Managers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace FuMarketAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IConnectionManager _connectionManager;

        public ChatsController(IHubContext<ChatHub> hubContext, IConnectionManager connectionManager)
        {
            _hubContext = hubContext;
            _connectionManager = connectionManager;
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDTO chatRequest)
        {
            try
            {
                HashSet<string>? connections = _connectionManager.GetConnections(chatRequest.ReceiverId);

                ChatMessage chatMessage = new ChatMessage()
                {
                    UserId = chatRequest.ReceiverId,
                    MessageContent = chatRequest.MessageContent,
                    Link = "",
                    DateCreated = DateTime.Now,
                    IsReaded = false,
                };

                if (connections != null)
                {
                    foreach (var connection in connections)
                    {
                        await _hubContext.Clients.Clients(connection).SendAsync("ReceiveMessage", new { SenderId = chatRequest.SenderId, MessageContent = chatRequest.MessageContent});
                    }
                }

                //_notificationRepositiory.AddNotification(notification);
                return Ok();
            }
            catch
            {
                return StatusCode(500);
            }

        }
    }
}
