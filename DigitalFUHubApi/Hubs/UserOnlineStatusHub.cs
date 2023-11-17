using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Managers;
using DigitalFUHubApi.Services;
using DTOs.Conversation;
using DTOs.Notification;
using DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace DigitalFUHubApi.Hubs
{
	[Authorize]
	public class UserOnlineStatusHub : Hub
	{
		private readonly IConnectionManager connectionManager;
		private readonly IConversationRepository conversationRepository;
		private readonly IUserRepository userRepository;
		private readonly HubService hubService;

		public UserOnlineStatusHub(IConnectionManager connectionManager, IConversationRepository conversationRepository, IUserRepository userRepository, HubService hubService)
		{
			this.connectionManager = connectionManager;
			this.conversationRepository = conversationRepository;
			this.userRepository = userRepository;
			this.hubService = hubService;
		}

		public override async Task OnConnectedAsync()
		{
			var userId = hubService.GetUserIdFromHubCaller(Context);

			//Update DB User
			userRepository.UpdateUserOnlineStatus(userId, true);

			// check user has been open in other divice
			var isUserConnectd = connectionManager.CheckUserConnected(userId, Constants.SIGNAL_R_USER_ONLINE_STATUS_HUB);

			// add new connection
			var currentConnectionId = hubService.GetConnectionIdFromHubCaller(Context);
			connectionManager.AddConnection(userId, Constants.SIGNAL_R_USER_ONLINE_STATUS_HUB, currentConnectionId);

			if (isUserConnectd) return;

			// get all user has conversation with current user
			List<UserConversationDTO> recipients = conversationRepository.GetRecipientUserIdHasConversation(userId);

			// send status online to all recipients online
			foreach (var recipient in recipients)
			{
				if (recipient.IsGroup)
				{
					int numberMemeberInGroupOnline = 0;
					var connectionIds = new List<string>();
					// count number user remaning existed online
					foreach (var memberUserId in recipient.MembersInGroup)
					{
						var isMemberOnline = connectionManager.CheckUserConnected(memberUserId, Constants.SIGNAL_R_USER_ONLINE_STATUS_HUB);
						if (isMemberOnline) 
						{ 
							numberMemeberInGroupOnline++;
							var connectionIdsOfMemberOnline = connectionManager.GetConnections(recipient.UserId, Constants.SIGNAL_R_USER_ONLINE_STATUS_HUB);
							if (connectionIdsOfMemberOnline == null) continue;
							connectionIds.AddRange(connectionIdsOfMemberOnline);
						}
					}
					if (numberMemeberInGroupOnline >= 1)
					{
						if (connectionIds == null || connectionIds.Count == 0) continue;

						foreach (var connectionId in connectionIds)
						{
							await SendUserOnlineStatus(recipient.ConversationId, true, connectionId, userId);
						}
					}
				}
				else
				{
					var isRecipientConnectd = connectionManager.CheckUserConnected(recipient.UserId, Constants.SIGNAL_R_USER_ONLINE_STATUS_HUB);
					if (!isRecipientConnectd) continue;

					var connectionIds = connectionManager.GetConnections(recipient.UserId, Constants.SIGNAL_R_USER_ONLINE_STATUS_HUB);
					if (connectionIds == null || connectionIds.Count == 0) continue;

					foreach (var connectionId in connectionIds)
					{
						await SendUserOnlineStatus(recipient.ConversationId, true, connectionId, userId);
					}
				}
			}

			
		}

		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			var userId = hubService.GetUserIdFromHubCaller(Context);
			var currentConnectionId = hubService.GetConnectionIdFromHubCaller(Context);
			//remove connection
			connectionManager.RemoveConnection(userId, Constants.SIGNAL_R_USER_ONLINE_STATUS_HUB, currentConnectionId);

			// check user has been open in orther divice
			var isUserConnectd = connectionManager.CheckUserConnected(userId, Constants.SIGNAL_R_USER_ONLINE_STATUS_HUB);
			if (isUserConnectd) return;

			// get all user has conversation with current user
			List<UserConversationDTO> recipients = conversationRepository.GetRecipientUserIdHasConversation(userId);
			// send status online to all recipients online
			foreach (var recipient in recipients)
			{
				var connectionIds = connectionManager.GetConnections(recipient.UserId, Constants.SIGNAL_R_USER_ONLINE_STATUS_HUB);
				if (connectionIds == null || connectionIds.Count == 0) continue;
				if (recipient.IsGroup)
				{
					int numberMemeberInGroupOnline = 0;
					// count number user remaning existed online
					foreach (var memberUserId in recipient.MembersInGroup)
					{
						var isMemberOnline = connectionManager.CheckUserConnected(memberUserId, Constants.SIGNAL_R_USER_ONLINE_STATUS_HUB);
						if(isMemberOnline) numberMemeberInGroupOnline++;	
					}
					if(numberMemeberInGroupOnline <= 1) 
					{
						foreach (var connectionId in connectionIds)
						{
							await SendUserOnlineStatus(recipient.ConversationId, false, connectionId, userId);
						}
					}
				}
				else
				{
					foreach (var connectionId in connectionIds)
					{
						await SendUserOnlineStatus(recipient.ConversationId, false, connectionId, userId);
					}
				}
				
			}

			//Update DB User
			userRepository.UpdateUserOnlineStatus(userId, false);

			return;
		}

		private async Task SendUserOnlineStatus(long conversationId, bool isOnline, string connectionId, long userId)
		{
			await Clients.Clients(connectionId)
				.SendAsync(Constants.SIGNAL_R_USER_ONLINE_STATUS_HUB_RECEIVE_ONLINE_STATUS,
					JsonConvert.SerializeObject(
						new UserOnlineStatusHubDTO { 
							ConversationId = conversationId,
							UserId = userId,
							IsOnline = isOnline,	
						})
				);
		}

	}
}
