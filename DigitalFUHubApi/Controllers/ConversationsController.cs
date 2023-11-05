using AutoMapper;
using DigitalFUHubApi.Hubs;
using DigitalFUHubApi.Managers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using DataAccess.IRepositories;
using Microsoft.AspNetCore.Authorization;
using DataAccess.Repositories;
using BusinessObject.Entities;
using DTOs.Conversation;
using DigitalFUHubApi.Services;
using Azure.Core;
using Azure;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Collections.Generic;
using DTOs.UserConversation;
using System.Threading;
using DigitalFUHubApi.Comons;
using Comons;
using DTOs.User;
using Newtonsoft.Json;

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
        private readonly IUserConversationRepository userConversationRepository;
        private readonly JwtTokenService jwtTokenService;

        public ConversationsController(IHubContext<ChatHub> hubContext, JwtTokenService jwtTokenService, IConnectionManager connectionManager, IConversationRepository conversationRepository, IMapper mapper, StorageService storageService, IUserRepository userRepository, IUserConversationRepository userConversationRepository)
        {
            this.hubContext = hubContext;
            this.connectionManager = connectionManager;
            this.conversationRepository = conversationRepository;
            this.mapper = mapper;
            this.storageService = storageService;
            this.userRepository = userRepository;
            this.userConversationRepository = userConversationRepository;
            this.jwtTokenService = jwtTokenService;
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromForm] SendMessageConversationRequestDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            ResponseData responseData = new ResponseData();
            string[] fileExtension = new string[] { ".jpge", ".png", ".jpg" };
            List<string> urlImages = new List<string>();
            List<string> connections = new List<string>();

            //check users recipient and user sender existed
            List<long> userIdInConverstion = new List<long>();
            userIdInConverstion.Add(request.UserId);
            userIdInConverstion.AddRange(request.RecipientIds);

            if (!userRepository.CheckUsersExisted(userIdInConverstion))
            {
                responseData.Status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
                responseData.Status.Ok = false;
                responseData.Status.Message = "Data not found!";
            }

            //get connectionId of user recipient
            foreach (long recipientId in request.RecipientIds)
            {
                // get connection id of chat hub
                HashSet<string>? connectionIds = connectionManager
                        .GetConnections(recipientId, Constants.SIGNAL_R_CHAT_HUB);

                if (connectionIds != null)
                {
                    connections.AddRange(connectionIds.ToList());
                }

            }

            //get connection id of user sender
            long senderId = request.UserId;
            HashSet<string>? connectionIdsSender = connectionManager
               .GetConnections(senderId, Constants.SIGNAL_R_CHAT_HUB);
            if (connectionIdsSender != null)
            {
                connections.AddRange(connectionIdsSender.ToList());
            }


            try
            {
                // upload file
                if (request.Images != null)
                {
                    if (request.Images.Any(x => !fileExtension.Contains(x.FileName.Substring(x.FileName.LastIndexOf(".")))))
                    {
                        return Ok(new Status
                        {
                            Ok = false,
                            ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT,
                            Message = "Invalid file!"
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

                // Create messages
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


               
                // add message
                await conversationRepository.SendMessageConversation(messages);

                // update un read for user recipient
                UpdateUserConversationRequestDTO requestUpdate = new UpdateUserConversationRequestDTO {
                    ConversationId = request.ConversationId,
                    IsRead = Constants.USER_CONVERSATION_TYPE_UN_READ,
                };

                foreach (long userId in request.RecipientIds)
                {
                    requestUpdate.UserId = userId;
                    userConversationRepository.Update(requestUpdate);
                }


                // message response to chat hub
                List<MessageConversationResponseDTO> messageConversations = mapper.Map<List<MessageConversationResponseDTO>>(messages);

                // signR chat hub
                if (connections.Count > 0)
                {
                    foreach (var connectionId in connections)
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

		[HttpPost("GetConversation")]
		public IActionResult GetConversation(GetConversationIdRequestDTO request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest();
			}
			try
			{
				ResponseData responseData = new ResponseData();
                var conversationId = conversationRepository.GetConversation(request.ShopId, request.UserId);
                if(conversationId == 0)
                {
					responseData.Status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
					responseData.Status.Ok = false;
					responseData.Status.Message = "Data not found!";
					return Ok(responseData);
				}
				responseData.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
				responseData.Status.Ok = true;
				responseData.Status.Message = "Success!";
                responseData.Result = conversationId;
				return Ok(responseData);
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return BadRequest(new Status());
            }
        }

        [HttpPost("add")]
        public IActionResult AddConversation([FromBody] AddConversationRequestDTO addConversation)
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

                //get connectionId of user recipient
                foreach (long recipientId in addConversation.RecipientIds)
                {
                    List<ConversationResponseDTO> conversationResponse = conversationRepository
                                                                        .GetUsersConversations(recipientId);
                    ConversationResponseDTO? conversation = conversationResponse.FirstOrDefault(x => x.ConversationId == conversationId);
                    if (conversation != null)
                    {
                        HashSet<string>? connectionIds = connectionManager
                                                            .GetConnections(recipientId, Constants.SIGNAL_R_CHAT_HUB);
                        if (connectionIds != null)
                        {
                            foreach (string connectionId in connectionIds)
                            {
                                hubContext.Clients.Clients(connectionId)
                                    .SendAsync(Constants.SIGNAL_R_CHAT_HUB_RECEIVE_MESSAGE, conversation);
                            }
                        }
                    }
                }

                return Ok(conversationId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return BadRequest(new Status());
            }
        }


        [HttpGet("getMessages")]
        public IActionResult GetMessages(long conversationId)
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

        [HttpGet("getNumberConversationUnRead")]
        [Authorize]
        public IActionResult GetNumberConversationUnRead(long userId)
        {
            ResponseData responseData = new ResponseData();
            Status status = new Status();
            try
            {

                if (userId == 0)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
                    status.Message = "Invalid";
                    status.Ok = false;
                    responseData.Status = status;
                    return Ok(responseData);
                }

                if (userId != jwtTokenService.GetUserIdByAccessToken(User))
                {
                    return Unauthorized();
                }

                var user = userRepository.GetUserById(userId);

                if (user == null)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
                    status.Message = "user not found!";
                    status.Ok = false;
                    responseData.Status = status;
                    return Ok(responseData);
                }

                long numberConversation = conversationRepository.GetNumberConversationUnReadOfUser(userId);
                status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
                status.Message = "Success";
                status.Ok = true;
                responseData.Status = status;
                responseData.Result = numberConversation;
                return Ok(responseData);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new Status());
            }
        }


    }
}