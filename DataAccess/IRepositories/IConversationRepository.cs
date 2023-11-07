
using BusinessObject.Entities;
using DTOs.Conversation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
    public interface IConversationRepository
    {
        List<ConversationResponseDTO> GetUsersConversations(long userId);
        Task SendMessageConversation(Message message);
        Conversation? GetConversationById(long conversationId);
        List<Message> GetMessages (long conversationId);
        long AddConversation(AddConversationRequestDTO addConversation);
        (string, string, bool) ValidateAddConversation(AddConversationRequestDTO addConversation);
		public List<UserConversationDTO> GetRecipientUserIdHasConversation(long userId);
        public long GetConversation(long shopId, long userId);
        long GetNumberConversationUnReadOfUser(long userId);
        List<long> GetConversationsUnRead(long userId);

    }
}
