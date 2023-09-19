using AutoMapper;
using BusinessObject;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DTOs;
using DigitalFUHubApi.Hubs;
using DigitalFUHubApi.Managers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace DigitalFUHubApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IConnectionManager _connectionManager;
        private readonly IChatRepository _chatRepository;
        private readonly IMapper _mapper;

        public ChatsController(IHubContext<ChatHub> hubContext, IConnectionManager connectionManager, 
            IChatRepository chatRepository, IMapper mapper)
        {
            _hubContext = hubContext;
            _connectionManager = connectionManager;
            _chatRepository = chatRepository;
            _mapper = mapper;
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] SendChatMessageRequestDTO sendChatMessageRequest)
        {
            try
            {
                int recipientId = unchecked((int)sendChatMessageRequest.RecipientId);
                HashSet<string>? connections = _connectionManager.GetConnections(recipientId);

                MessageResponseDTO messageResponse = new MessageResponseDTO
                {
                    UserId = sendChatMessageRequest.SenderId,
                    ConversationId = sendChatMessageRequest.ConversationId,
                    Content = sendChatMessageRequest.Content,
                    DateCreate = sendChatMessageRequest.DateCreate
                };
                if (connections != null)
                {
                    foreach (var connection in connections)
                    {
                        await _hubContext.Clients.Clients(connection).SendAsync("ReceiveMessage", messageResponse);
                    }
                }

                await _chatRepository.SendChatMessage(sendChatMessageRequest);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }

        }

        [HttpGet("getSenders")]
        public async Task<IActionResult> GetSendersConversation(long userId, int page = 1, int limit = 10)
        {
            try
            {
                List<SenderConversation> senderConversations = await _chatRepository.GetSenderConversations(userId, page, limit);
                return Ok(senderConversations);
            } catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }


        [HttpGet("getListMessage")]
        public async Task<IActionResult> GetListMessage(long conversationId)
        {
            try
            {
                List<MessageResponseDTO> messages = _mapper.Map<List<MessageResponseDTO>>(await _chatRepository.GetListMessage(conversationId));
                return Ok(messages);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
