
using BusinessObject.Entities;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using DTOs.Chat;
using DTOs.Conversation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        public List<Message> GetMessages(long conversationId) => ConversationDAO.Instance.GetMessages(conversationId);

        public List<ConversationResponseDTO> GetUsersConversations(long userId) => ConversationDAO.Instance.GetSenderConversations(userId);



        public bool GetUserConversation(long senderId, long recipientId)
        {
            if (senderId == 0 || recipientId == 0)
            {
                throw new ArgumentException("senderId or recipientId invalid");
            }
            bool result = false;
            List<UserConversation> userConversations = ConversationDAO.Instance.GetUserConversation(senderId, recipientId);
            if (userConversations.Count == 2)
            {
                result = true;
            }

            return result;
        }

        public async Task SendMessageConversation(List<Message> messages) => await ConversationDAO.Instance.SendMessageConversation(messages);

        public long AddConversation(AddConversationRequestDTO addConversation) => ConversationDAO.Instance.AddConversation(addConversation);

        public (bool, string) ValidateAddConversation(AddConversationRequestDTO addConversation) => ConversationDAO.Instance.ValidateAddConversation(addConversation);

		public List<UserConversationDTO> GetRecipientUserIdHasConversation(long userId) => ConversationDAO.Instance.GetRecipientUserIdHasConversation(userId);
	}
}
