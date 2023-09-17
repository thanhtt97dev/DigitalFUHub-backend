using BusinessObject;
using DataAccess.IRepositories;
using DataAccess.Repositories;
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
        private readonly IChatRepository _chatRepository;

        public ChatsController(IHubContext<ChatHub> hubContext, IConnectionManager connectionManager, IChatRepository chatRepository)
        {
            _hubContext = hubContext;
            _connectionManager = connectionManager;
            _chatRepository = chatRepository;
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDTO chatRequest)
        {
            try
            {
                int recipientId = unchecked((int)chatRequest.RecipientId);
                HashSet<string>? connections = _connectionManager.GetConnections(recipientId);

                if (connections != null)
                {
                    foreach (var connection in connections)
                    {
                        await _hubContext.Clients.Clients(connection).SendAsync("ReceiveMessage", new { SenderId = chatRequest.SenderId, MessageContent = chatRequest.Content});
                    }
                }

                await _chatRepository.SendChatMessage(chatRequest);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }

        }

        [HttpGet("getSenders/{userId}")]
        public async Task<IActionResult> GetSendersConversation([FromRoute] long userId)
        {
            try
            {
                List<SenderConversation> senderConversations = await _chatRepository.GetSenderConversations(userId);
                return Ok(senderConversations);
            } catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
