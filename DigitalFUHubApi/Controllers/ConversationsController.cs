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
        private readonly IHubContext<ChatHub> hubContext;
        private readonly IConnectionManager connectionManager;
        private readonly IConversationRepository conversationRepository;
        private readonly IMapper mapper;
        private readonly StorageService storageService;
        private readonly IUserRepository userRepository;

		public ConversationsController(IHubContext<ChatHub> hubContext, IConnectionManager connectionManager, IConversationRepository conversationRepository, IMapper mapper, StorageService storageService, IUserRepository userRepository)
		{
			this.hubContext = hubContext;
			this.connectionManager = connectionManager;
			this.conversationRepository = conversationRepository;
			this.mapper = mapper;
			this.storageService = storageService;
			this.userRepository = userRepository;
		}

		[HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromForm] SendMessageConversationRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
			ResponseData responseData = new ResponseData();

			//check users recipient and user sender existed
			List<long> userIdInConverstion = new List<long>();
			userIdInConverstion.Add(request.UserId);
			userIdInConverstion.AddRange(request.RecipientIds);

			if (!userRepository.CheckUsersExisted(userIdInConverstion)){
				responseData.Status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
				responseData.Status.Ok = false;
				responseData.Status.Message = "Data not found!";
			}

			string[] fileExtension = new string[] { ".jpge", ".png", ".jpg" };
            List<string> urlImages = new List<string>();
            List<string> connect = new List<string>();
            List<HashSet<string>?> connections = new List<HashSet<string>?>();

			// user recipient
			foreach (long recipientId in request.RecipientIds)
            {
                HashSet<string>? connection = connectionManager
                        .GetConnections(recipientId, Constants.SIGNAL_R_CHAT_HUB);
                //connections.Add(connection);
                if (connection == null) continue;
				connect.AddRange(connection.ToList());
			}
			
			// user sender
			long senderId = request.UserId;
            HashSet<string>? connectionSender = connectionManager
               .GetConnections(senderId, Constants.SIGNAL_R_CHAT_HUB);
            connections.Add(connectionSender);
			if (connectionSender != null)
            {
				connect.AddRange(connectionSender.ToList());
			}

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
                        string url = await storageService.UploadFileToAzureAsync(file, filename);
                        urlImages.Add(url);
                    }
                }

                // Create message
                Message newMessage;
                List<Message> messages = new List<Message>();

                if (urlImages.Count > 0)
                {
                    foreach (string url in urlImages)
                    {
                        newMessage = new Message
                        {
                            UserId = request.UserId,
                            ConversationId = request.ConversationId,
                            Content = url,
                            MessageType = Constants.MESSAGE_TYPE_CONVERSATION_IMAGE,
                            DateCreate = DateTime.Now,
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
                        DateCreate = DateTime.Now,
                        IsDelete = false
                    };
                    messages.Add(newMessage);
                }

                await conversationRepository.SendMessageConversation(messages);

                List<MessageConversationResponseDTO> messageConversations = mapper.Map<List<MessageConversationResponseDTO>>(messages);

                if(connect.Count > 0)
                {
                    foreach (var connectionId in connect)
                    {
						foreach (var msg in messageConversations)
						{
							await hubContext.Clients.Clients(connectionId)
						   .SendAsync(Constants.SIGNAL_R_CHAT_HUB_RECEIVE_MESSAGE, msg);
						}
					}
                }

                return Ok();
            }
            catch (Exception ex)
            {
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
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
                List<ConversationResponseDTO> userConversations = conversationRepository.GetUsersConversations(userId);

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
                (bool, string) result = conversationRepository.ValidateAddConversation(addConversation);
                if (!result.Item1)
                {
                    Console.WriteLine(result.Item2);
                    return BadRequest(new Status()); 
                }
                long conversationId = conversationRepository.AddConversation(addConversation);

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
                List<MessageConversationResponseDTO> messages = mapper
                    .Map<List<MessageConversationResponseDTO>>(conversationRepository.GetMessages(conversationId));
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
                return Ok(conversationRepository.GetUserConversation(senderId, recipientId));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

    }
}
