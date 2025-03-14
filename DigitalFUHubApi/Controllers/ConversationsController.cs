﻿using AutoMapper;
using DigitalFUHubApi.Hubs;
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
using DigitalFUHubApi.Managers.IRepositories;

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
        private readonly AzureStorageAccountService azureStorageAccountService;
        private readonly IUserRepository userRepository;
        private readonly IUserConversationRepository userConversationRepository;
        private readonly JwtTokenService jwtTokenService;
        private readonly IShopRepository shopRepository;

        public ConversationsController(IHubContext<ChatHub> hubContext, JwtTokenService jwtTokenService, IConnectionManager connectionManager, IConversationRepository conversationRepository, IMapper mapper, AzureStorageAccountService azureStorageAccountService, IUserRepository userRepository, IUserConversationRepository userConversationRepository, IShopRepository shopRepository)
        {
            this.hubContext = hubContext;
            this.connectionManager = connectionManager;
            this.conversationRepository = conversationRepository;
            this.mapper = mapper;
            this.azureStorageAccountService = azureStorageAccountService;
            this.userRepository = userRepository;
            this.userConversationRepository = userConversationRepository;
            this.jwtTokenService = jwtTokenService;
			this.shopRepository = shopRepository;
        }

		#region Send message
		[Authorize]
		[HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromForm] SendMessageConversationRequestDTO request)
        {
             
            if (!ModelState.IsValid) return BadRequest();

            if (request.UserId != jwtTokenService.GetUserIdByAccessToken(User))
            {
                return Unauthorized();
            }

            // Check conversation existed
            var conversation = conversationRepository.GetConversationById(request.ConversationId);
            if (conversation == null)
            {
				return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Data not found!", false, new()));
			}

            // Check users recipient and user sender existed
            List<long> userIdInConverstion = new List<long>();
            userIdInConverstion.Add(request.UserId);
            userIdInConverstion.AddRange(request.RecipientIds);

            bool resultCheckUsersExisted = userRepository.CheckUsersExisted(userIdInConverstion);
            if (!resultCheckUsersExisted)
            {
				return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Data not found!", false, new()));
			}

            // Check sends many types of messages at once
            if (request.Content != null && request.Image != null)
            {
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Cannot send multiple types of messages at the same time!", false, new()));
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
						return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid file!", false, new()));
                    }

                    // Declares variable
                    DateTime now;
                    string filename = "";

                    // Upload file to azure
                    now = DateTime.Now;
                    filename = string.Format("{0}_{1}{2}{3}{4}{5}{6}{7}{8}", request.UserId, now.Year, now.Month, now.Day, now.Millisecond, now.Second, now.Minute, now.Hour, fileRequest.FileName.Substring(fileRequest.FileName.LastIndexOf(".")));
                    urlImage = await azureStorageAccountService.UploadFileToAzureAsync(fileRequest, filename);
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
                        IsDelete = false,
                    };
                } else {
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Message is not allowed to be null!", false, new()));
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

				// get role id of user
				messageConversation.RoleId = userRepository.GetRoleIdUser(request.UserId);

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
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success!", true, new()));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
		#endregion

		#region Get conversation id
		[Authorize]
		[HttpPost("GetConversation")]
		public IActionResult GetConversation(GetConversationIdRequestDTO request)
		{
			if (!ModelState.IsValid) return BadRequest();
			try
			{
				var conversationId = conversationRepository.GetConversation(request.ShopId, request.UserId);
				if (conversationId == 0)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Data not found!", false, new()));
				}
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success!", true, conversationId));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}

		}
		#endregion

		#region Get list conversation of a user
		[Authorize]
		[HttpGet("getConversations")]
		public IActionResult GetConversations(long userId)
		{
			try
			{
				if (userId == 0) return BadRequest();
				if (userId != jwtTokenService.GetUserIdByAccessToken(User)) return Unauthorized();

				var user = userRepository.GetUserById(userId);
				if (user == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Data not found!", false, new()));
				}

				List<ConversationResponseDTO> userConversations = conversationRepository.GetUsersConversations(userId);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success!", true, userConversations));

			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Add new conversation
		[Authorize]
		[HttpPost("add")]
		public IActionResult AddConversation([FromBody] AddConversationRequestDTO addConversation)
		{
			try
			{
				if (addConversation.UserId != jwtTokenService.GetUserIdByAccessToken(User)) return Unauthorized();

				(string responseCode, string message, bool isOk) = conversationRepository.ValidateAddConversation(addConversation);

				if (!isOk)
				{
					return Ok(new ResponseData(responseCode, message, isOk, new()));
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

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, conversationId));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get messages of a conversation
		[Authorize]
		[HttpGet("getMessages")]
		public IActionResult GetMessages(long conversationId)
		{
			try
			{
				if (conversationId == 0)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid", false, new {}));
				}

				// Check conversation existed
				var conversation = conversationRepository.GetConversationById(conversationId);

				if (conversation == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Data not found", false, new { }));
				}

				List<MessageConversationResponseDTO> messages = mapper
								.Map<List<MessageConversationResponseDTO>>(conversationRepository.GetMessages(conversationId));
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, messages));
			}
			catch (ArgumentException ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get number conversation un read
		[Authorize]
		[HttpGet("getNumberConversationUnRead")]
		public IActionResult GetNumberConversationUnRead(long userId)
		{
			try
			{
				if (userId == 0) return BadRequest();

				if (userId != jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}

				var user = userRepository.GetUserById(userId);
				if (user == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Data not found", false, new { }));
				}

				long numberConversation = conversationRepository.GetNumberConversationUnReadOfUser(userId);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, numberConversation));
			}
			catch (ArgumentException ex)
			{
				Console.WriteLine(ex.Message);
				return BadRequest(new Status());
			}
		}
		#endregion

		#region Get conversation un read detail
		[HttpGet("GetConversationsUnRead/{userId}")]
		public IActionResult GetConversationsUnRead(long userId)
		{
			try
			{
				if (userId == 0)
				{
					return BadRequest();
				}

				var result = conversationRepository.GetConversationsUnRead(userId);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, result));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion
	}
}