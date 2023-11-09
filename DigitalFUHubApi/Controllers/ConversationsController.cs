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
        [Authorize]
        public async Task<IActionResult> SendMessage([FromForm] SendMessageConversationRequestDTO request)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // Response
            ResponseData responseData = new ResponseData();
            Status status = new Status();

            if (request.UserId != jwtTokenService.GetUserIdByAccessToken(User))
            {
                return Unauthorized();
            }

            // Check conversation existed
            var conversation = conversationRepository.GetConversationById(request.ConversationId);
            if (conversation == null)
            {
                status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
                status.Ok = false;
                status.Message = "Data not found!";
                responseData.Status = status;
                return Ok(responseData);
            }

            // Check users recipient and user sender existed
            List<long> userIdInConverstion = new List<long>();
            userIdInConverstion.Add(request.UserId);
            userIdInConverstion.AddRange(request.RecipientIds);

            bool resultCheckUsersExisted = userRepository.CheckUsersExisted(userIdInConverstion);
            if (!resultCheckUsersExisted)
            {
                status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
                status.Ok = false;
                status.Message = "Data not found!";
                responseData.Status = status;
                return Ok(responseData);
            }

            // Check sends many types of messages at once
            if (request.Content != null && request.Image != null)
            {
                status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
                status.Ok = false;
                status.Message = "Cannot send multiple types of messages at the same time!";
                responseData.Status = status;
                return Ok(responseData);
            }

            // Declares variable
            string[] fileExtension = new string[] { ".jpge", ".png", ".jpg" };
            List<string> connections = new List<string>();
            string urlImage = "";

            // Get connectionId of user recipient
            foreach (long recipientId in request.RecipientIds)
            {
                // Get connection id of chat hub
                HashSet<string>? connectionIds = connectionManager
                        .GetConnections(recipientId, Constants.SIGNAL_R_CHAT_HUB);

                if (connectionIds != null) connections.AddRange(connectionIds.ToList());
            }

            // Get connection id of user sender
            HashSet<string>? connectionIdsSender = connectionManager
               .GetConnections(request.UserId, Constants.SIGNAL_R_CHAT_HUB);
            if (connectionIdsSender != null) connections.AddRange(connectionIdsSender.ToList());

            try
            {
                // Upload file
                if (request.Image != null)
                {
                    // Check file extension
                    IFormFile fileRequest = request.Image;
                    if (!fileExtension.Contains(fileRequest.FileName.Substring(fileRequest.FileName.LastIndexOf("."))))
                    {
                        status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
                        status.Ok = false;
                        status.Message = "Invalid file!";
                        responseData.Status = status;
                        return Ok(responseData);
                    }

                    // Declares variable
                    DateTime now;
                    string filename = "";

                    // Upload file to azure
                    now = DateTime.Now;
                    filename = string.Format("{0}_{1}{2}{3}{4}{5}{6}{7}{8}", request.UserId, now.Year, now.Month, now.Day, now.Millisecond, now.Second, now.Minute, now.Hour, fileRequest.FileName.Substring(fileRequest.FileName.LastIndexOf(".")));
                    urlImage = await storageService.UploadFileToAzureAsync(fileRequest, filename);
                }

                // Declares variable
                Message newMessage;

                // Create message
                if (!string.IsNullOrEmpty(urlImage))
                {
                    // Create message type image
                    newMessage = new Message
                    {
                        UserId = request.UserId,
                        ConversationId = request.ConversationId,
                        Content = urlImage,
                        MessageType = Constants.MESSAGE_TYPE_CONVERSATION_IMAGE,
                        DateCreate = DateTime.Now,
                        IsDelete = false
                    };
                } else if (!string.IsNullOrEmpty(request.Content)) {
                    // Create message type text
                    newMessage = new Message
                    {
                        UserId = request.UserId,
                        ConversationId = request.ConversationId,
                        Content = request.Content,
                        MessageType = Constants.MESSAGE_TYPE_CONVERSATION_TEXT,
                        DateCreate = DateTime.Now,
                        IsDelete = false
                    };
                } else {
                    status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
                    status.Ok = false;
                    status.Message = "Message is not allowed to be null!";
                    responseData.Status = status;
                    return Ok(responseData);
                }

               
                // Add message to db
                await conversationRepository.SendMessageConversation(newMessage);


                UpdateUserConversationRequestDTO requestUpdate = new UpdateUserConversationRequestDTO {
                    ConversationId = request.ConversationId,
                    IsRead = Constants.USER_CONVERSATION_TYPE_UN_READ,
                };

                // Update un read for user recipient
                foreach (long userId in request.RecipientIds)
                {
                    requestUpdate.UserId = userId;
                    userConversationRepository.Update(requestUpdate);
                }


                // Message response to chat hub
                MessageConversationResponseDTO messageConversation = mapper.Map<MessageConversationResponseDTO>(newMessage);

                // Send to signR chat hub
                if (connections.Count > 0)
                {
                    foreach (var connectionId in connections)
                    {
                        await hubContext.Clients.Clients(connectionId)
                           .SendAsync(Constants.SIGNAL_R_CHAT_HUB_RECEIVE_MESSAGE, messageConversation);

                    }
                }

                // Ok
                status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
                status.Ok = true;
                status.Message = "Success";
                responseData.Status = status;
                return Ok(responseData);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

        }

		[HttpPost("GetConversation")]
        [Authorize]
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

		[HttpGet("getConversations")]
        [Authorize]
        public IActionResult GetConversations(long userId)
        {
            ResponseData responseData = new ResponseData();
            Status status = new Status();
            try
            {
                if (userId == 0) {
                    status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
                    status.Ok = false;
                    status.Message = "Invalid file!";
                    responseData.Status = status;
                    return Ok(responseData);
                }

                var user = userRepository.GetUserById(userId);
                if (user == null)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
                    status.Ok = false;
                    status.Message = "Data not found!";
                    responseData.Status = status;
                    return Ok(responseData);
                }

                if (userId != jwtTokenService.GetUserIdByAccessToken(User))
                {
                    return Unauthorized();
                }

                List<ConversationResponseDTO> userConversations = conversationRepository.GetUsersConversations(userId);
                responseData.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
                responseData.Status.Ok = true;
                responseData.Status.Message = "Success!";
                responseData.Result = userConversations;
                return Ok(responseData);

            } catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("add")]
        [Authorize]
        public IActionResult AddConversation([FromBody] AddConversationRequestDTO addConversation)
        {
            ResponseData responseData = new ResponseData();
            Status status = new Status();
            try
            {

                if (addConversation.UserId != jwtTokenService.GetUserIdByAccessToken(User))
                {
                    return Unauthorized();
                }

                (string responseCode, string message, bool isOk) = conversationRepository.ValidateAddConversation(addConversation);
                
                if (!isOk)
                {
                    status.ResponseCode = responseCode;
                    status.Message = message;
                    status.Ok = isOk;
                    responseData.Status = status;
                    return Ok(responseData);
                }

                // Add conversation
                long conversationId = conversationRepository.AddConversation(addConversation);

                // Get connectionId of user recipient
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

                status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
                status.Message = "Success";
                status.Ok = true;
                responseData.Status = status;
                responseData.Result = conversationId;
                return Ok(conversationId);
            }
            catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        [HttpGet("getMessages")]
        [Authorize]
        public IActionResult GetMessages(long conversationId)
        {
            ResponseData responseData = new ResponseData();
            Status status = new Status();
            try
            {
                if (conversationId == 0)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
                    status.Ok = false;
                    status.Message = "Invalid!";
                    responseData.Status = status;
                    return Ok(responseData);
                }

                // Check conversation existed
                var conversation = conversationRepository.GetConversationById(conversationId);

                if (conversation == null)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
                    status.Ok = false;
                    status.Message = "Data not found!";
                    responseData.Status = status;
                    return Ok(responseData);
                }

                List<MessageConversationResponseDTO> messages = mapper
                                .Map<List<MessageConversationResponseDTO>>(conversationRepository.GetMessages(conversationId));
                status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
                status.Message = "Success";
                status.Ok = true;
                responseData.Status = status;
                responseData.Result = messages;
                return Ok(responseData);
            }
            catch (ArgumentException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
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

		[HttpGet("GetConversationsUnRead/{userId}")]
		public IActionResult GetConversationsUnRead(long userId)
        {
			ResponseData responseData = new ResponseData();
			Status status = new Status();
            try
            {
                if (userId == 0)
                {
                    return BadRequest();
                }

                var result = conversationRepository.GetConversationsUnRead(userId);

				status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
				status.Message = "Success";
				status.Ok = true;
				responseData.Status = status;
				responseData.Result = result;
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}


    }
}