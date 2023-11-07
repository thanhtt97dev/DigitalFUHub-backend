
using BusinessObject.Entities;
using DataAccess.DAOs;
using DataAccess.IRepositories;
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

        public async Task SendMessageConversation(Message message) => await ConversationDAO.Instance.SendMessageConversation(message);

        public long AddConversation(AddConversationRequestDTO addConversation) => ConversationDAO.Instance.AddConversation(addConversation);

        public (string, string, bool) ValidateAddConversation(AddConversationRequestDTO addConversation) => ConversationDAO.Instance.ValidateAddConversation(addConversation);

		public List<UserConversationDTO> GetRecipientUserIdHasConversation(long userId) => ConversationDAO.Instance.GetRecipientUserIdHasConversation(userId);

		public long GetConversation(long shopId, long userId) => ConversationDAO.Instance.GetConversation(shopId, userId);

        public long GetNumberConversationUnReadOfUser(long userId) => ConversationDAO.Instance.GetNumberConversationUnReadOfUser(userId);

		public List<long> GetConversationsUnRead(long userId) => ConversationDAO.Instance.GetConversationsUnRead(userId);

        public Conversation? GetConversationById(long conversationId) => ConversationDAO.Instance.GetConversationById(conversationId);
    }
}
