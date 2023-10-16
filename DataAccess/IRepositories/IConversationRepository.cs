
using BusinessObject.Entities;
using DTOs.Chat;
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

        Task SendMessageConversation(List<Message> messages);

        List<Message> GetMessages (long conversationId);

        bool GetUserConversation(long senderId, long recipientId);

        long AddConversation(AddConversationRequestDTO addConversation);
        (bool, string) ValidateAddConversation(AddConversationRequestDTO addConversation);
    }
}
