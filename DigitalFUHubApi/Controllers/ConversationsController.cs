using AutoMapper;
using DigitalFUHubApi.Hubs;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Managers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using DTOs.Chat;
using DataAccess.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Comons;
using DataAccess.Repositories;
using BusinessObject.Entities;
using DTOs.Conversation;
using DigitalFUHubApi.Services;
using Azure.Core;
using Azure;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
        private readonly StorageService _storageService;

        public ConversationsController(IHubContext<ChatHub> hubContext, IConnectionManager connectionManager, 
            IConversationRepository conversationRepository, IMapper mapper, StorageService storageService)
        {
            _hubContext = hubContext;
            _connectionManager = connectionManager;
            _conversationRepository = conversationRepository;
            _mapper = mapper;
            _storageService = storageService;
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromForm] SendMessageConversationRequestDTO request)
        {
            if (request == null)
            {
                return BadRequest(new Status());
            }

            string[] fileExtension = new string[] { ".jpge", ".png", ".jpg" };
            List<string> urlImages = new List<string>();
            List<HashSet<string>?> connections = new List<HashSet<string>?>();

            // user recipient
            foreach (long userId in request.RecipientIds)
            {
                int recipientId = unchecked((int)userId);
                HashSet<string>? connection = _connectionManager
                        .GetConnections(recipientId, Constants.SIGNAL_R_CHAT_HUB);
                connections.Add(connection);
            }

            // user sender
            int senderId = unchecked((int)request.UserId);
            HashSet<string>? connectionSender = _connectionManager
               .GetConnections(senderId, Constants.SIGNAL_R_CHAT_HUB);
            connections.Add(connectionSender);

            try
            {
                if (request.Images != null) {
                    if (request.Images.Any(x => !fileExtension.Contains(x.FileName.Substring(x.FileName.LastIndexOf("."))))) {
                        Console.WriteLine("File không hợp lệ");
                        return Ok(new Status {
                            Ok = false,
                            ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT,
                            Message = ""
                        });
                    }

                    DateTime now;
                    string filename;

                    foreach (IFormFile file in request.Images)
                    {
                        now = DateTime.Now;
                        filename = string.Format("{0}_{1}{2}{3}{4}{5}{6}{7}{8}", request.UserId, now.Year, now.Month, now.Day, now.Millisecond, now.Second, now.Minute, now.Hour, file.FileName.Substring(file.FileName.LastIndexOf(".")));
                        string url = await _storageService.UploadFileToAzureAsync(file, filename);
                        urlImages.Add(url);
                    }
                }

                // Create message
                Message newMessage;
                List<Message> messages = new List<Message>();

                if (urlImages != null && urlImages.Count > 0)
                {
                    foreach (string url in urlImages)
                    {
                        newMessage = new Message
                        {
                            UserId = request.UserId,
                            ConversationId = request.ConversationId,
                            Content = url,
                            MessageType = Constants.MESSAGE_TYPE_CONVERSATION_IMAGE,
                            DateCreate = request.DateCreate,
                            IsDelete = false
                        };
                        messages.Add(newMessage);
                    }
                }

                if (!string.IsNullOrEmpty(request.Content))
                {
                    newMessage = new Message
                    {
                        UserId = request.UserId,
                        ConversationId = request.ConversationId,
                        Content = request.Content,
                        MessageType = Constants.MESSAGE_TYPE_CONVERSATION_TEXT,
                        DateCreate = request.DateCreate,
                        IsDelete = false
                    };
                    messages.Add(newMessage);
                }

                await _conversationRepository.SendMessageConversation(messages);

                List<MessageConversationResponseDTO> messageConversations = _mapper.Map<List<MessageConversationResponseDTO>>(messages);


              
                if (connections.Count > 0)
                {
                    
                    for(int i = 0; i < connections.Count; i++)
                    {
                        var elementConnection = connections.ElementAt(i);
                        if (elementConnection != null)
                        {
                            foreach (var item in elementConnection)
                            {
                                foreach(var msg in messageConversations)
                                {
                                    await _hubContext.Clients.Clients(item)
                                   .SendAsync(Constants.SIGNAL_R_CHAT_HUB_RECEIVE_MESSAGE, msg);
                                }
       
                            }
                        }
                    }
                }


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
                if (userId == 0)
                {
                    return BadRequest(new Status());
                }
                List<ConversationResponseDTO> userConversations = _conversationRepository.GetUsersConversations(userId);

                return Ok(userConversations);
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return BadRequest(new Status());
            }
        }

        [HttpPost("add")]
        public IActionResult AddConversation ([FromBody] AddConversationRequestDTO addConversation)
        {
            try
            {
                (bool, string) result = _conversationRepository.ValidateAddConversation(addConversation);
                if (!result.Item1)
                {
                    Console.WriteLine(result.Item2);
                    return BadRequest(new Status()); 
                }
                long conversationId = _conversationRepository.AddConversation(addConversation);

                return Ok(conversationId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return BadRequest(new Status());
            }
        }


        [HttpGet("getMessages")]
        public IActionResult GetMessages (long conversationId)
        {
            try
            {
                if (conversationId == 0)
                {
                    return BadRequest(new Status());
                }
                List<MessageConversationResponseDTO> messages = _mapper
                    .Map<List<MessageConversationResponseDTO>>(_conversationRepository.GetMessages(conversationId));
                return Ok(messages);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new Status());
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
