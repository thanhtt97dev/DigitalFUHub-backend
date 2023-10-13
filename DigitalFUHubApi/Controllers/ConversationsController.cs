using AutoMapper;
using DigitalFUHubApi.Hubs;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Managers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using DTOs.Chat;
using DataAccess.IRepositories;
using BusinessObject.DataTransfer;
using Microsoft.AspNetCore.Authorization;
using Comons;
using DataAccess.Repositories;
using BusinessObject.Entities;
using DTOs.Conversation;

namespace DigitalFUHubApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationsController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IConnectionManager _connectionManager;
        private readonly IConversationRepository _conversationRepository;
        private readonly IMapper _mapper;

        public ConversationsController(IHubContext<ChatHub> hubContext, IConnectionManager connectionManager, 
            IConversationRepository conversationRepository, IMapper mapper)
        {
            _hubContext = hubContext;
            _connectionManager = connectionManager;
            _conversationRepository = conversationRepository;
            _mapper = mapper;
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromForm] SendChatMessageRequestDTO sendChatMessageRequest)
        {
            try
            {
                if (!string.IsNullOrEmpty(sendChatMessageRequest.Content))
                {
                    int recipientId = unchecked((int)sendChatMessageRequest.RecipientId);
                    HashSet<string>? connections = _connectionManager
                        .GetConnections(recipientId, Constants.SIGNAL_R_CHAT_HUB);

                    MessageResponseDTO messageResponse = new MessageResponseDTO
                    {
                        UserId = sendChatMessageRequest.SenderId,
                        ConversationId = sendChatMessageRequest.ConversationId,
                        Content = sendChatMessageRequest.Content,
                        DateCreate = sendChatMessageRequest.DateCreate,
                        MessageType = sendChatMessageRequest.MessageType
                    };
                    if (connections != null)
                    {
                        foreach (var connection in connections)
                        {
                            await _hubContext.Clients.Clients(connection)
                                .SendAsync(Constants.SIGNAL_R_CHAT_HUB_RECEIVE_MESSAGE, messageResponse);
                        }
                    }
                }
               
                await _conversationRepository.SendChatMessage(sendChatMessageRequest);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }

        }

        [HttpGet("getUsers")]
        public IActionResult GetUsersConversation(long userId)
        {
            try
            {
                List<ConversationResponseDTO> userConversations = _conversationRepository.GetUsersConversations(userId);

                return Ok(userConversations);
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return BadRequest(new Status());
            }
        }


        [HttpGet("getListMessage")]
        public async Task<IActionResult> GetListMessage(long conversationId)
        {
            try
            {
                List<MessageResponseDTO> messages = _mapper
                    .Map<List<MessageResponseDTO>>(await _conversationRepository.GetListMessage(conversationId));
                return Ok(messages);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }


        [HttpGet("existUserConversation")]
        public IActionResult ExistUserConversation(long senderId, long recipientId)
        {
            try
            {
                return Ok(_conversationRepository.GetUserConversation(senderId, recipientId));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

    }
}
