using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Managers;
using DigitalFUHubApi.Services;
using DTOs.Conversation;
using DTOs.Notification;
using DTOs.User;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace DigitalFUHubApi.Hubs
{
	public class UserOnlineStatusHub : Hub
	{
		private readonly HubConnectionService hubConnectionService;
		private readonly ConnectionManager connectionManager;
		private readonly ConversationRepository conversationRepository;

		public UserOnlineStatusHub(HubConnectionService hubConnectionService, ConnectionManager connectionManager, ConversationRepository conversationRepository)
		{
			this.hubConnectionService = hubConnectionService;
			this.connectionManager = connectionManager;
			this.conversationRepository = conversationRepository;
		}

		public override async Task OnConnectedAsync()
		{

			var userId = hubConnectionService.GetUserIdFromHubCaller(Context);
			// check user has been open in orther divice
			var isUserConnectd = hubConnectionService.CheckUserConnected(userId);
			if (isUserConnectd) return;

			// add new connection
			hubConnectionService.AddConnection(Context, Constants.SIGNAL_R_USER_ONLINE_STATUS_HUB);

			// get all user has conversation with current user
			List<UserConversationDTO> recipients = conversationRepository.GetRecipientUserIdHasConversation(userId);

			// send status online to all recipients online
			foreach (var recipient in recipients)
			{
				var connectionIds = connectionManager.GetConnections(userId, Constants.SIGNAL_R_USER_ONLINE_STATUS_HUB);
				if (connectionIds == null || connectionIds.Count == 0) continue;
				foreach (var connectionId in connectionIds)
				{
				 	await SendUserOnlineStatus(recipient.ConversationId, true, connectionId);
				}
			}

			//Update DB User
		}

		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			//remove connection
			hubConnectionService.RemoveConnection(Context, Constants.SIGNAL_R_USER_ONLINE_STATUS_HUB_RECEIVE_ONLINE_STATUS);

			var userId = hubConnectionService.GetUserIdFromHubCaller(Context);

			// check user has been open in orther divice
			var isUserConnectd = hubConnectionService.CheckUserConnected(userId);
			if (isUserConnectd) return;

			// get all user has conversation with current user
			List<UserConversationDTO> recipients = conversationRepository.GetRecipientUserIdHasConversation(userId);
			// send status online to all recipients online
			foreach (var recipient in recipients)
			{
				var connectionIds = connectionManager.GetConnections(userId, Constants.SIGNAL_R_USER_ONLINE_STATUS_HUB);
				if (connectionIds == null || connectionIds.Count == 0) continue;
				if (recipient.IsGroup)
				{
					int numberMemeberInGroupOnline = 0;
					// count number user remaning existed online
					foreach (var member in recipient.MembersInGroup)
					{
						var isMemberOnline = hubConnectionService.CheckUserConnected(userId);
						if(isMemberOnline) numberMemeberInGroupOnline++;	
					}
					if(numberMemeberInGroupOnline == 0) 
					{
						foreach (var connectionId in connectionIds)
						{
							await SendUserOnlineStatus(recipient.ConversationId, false, connectionId);
						}
					}
				}
				else
				{
					foreach (var connectionId in connectionIds)
					{
						await SendUserOnlineStatus(recipient.ConversationId, false, connectionId);
					}
				}
				
			}

			//Update DB User


			return;
		}

		private async Task SendUserOnlineStatus(long conversationId, bool isOnline, string connectionId)
		{
			await Clients.Clients(connectionId)
				.SendAsync(Constants.SIGNAL_R_USER_ONLINE_STATUS_HUB_RECEIVE_ONLINE_STATUS,
					JsonConvert.SerializeObject(
						new UserOnlineStatusHubDTO { 
							ConversationId = conversationId,
							IsOnline = isOnline,	
						})
				);
		}

	}
}
