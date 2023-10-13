using BusinessObject.DataTransfer;
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
        public async Task<List<Message>> GetListMessage(long conversationId)
        {
            if (conversationId == 0)
            {
                throw new ArgumentException("conversationId = 0, Can not get list message");
            }

            return await ConversationDAO.Instance.GetListMessage(conversationId);
        }

        public List<ConversationResponseDTO> GetUsersConversations(long userId)
        {
            if (userId == 0)
            {
                throw new ArgumentException("UserId = 0, Can not get sender conversations");
            }

            return ConversationDAO.Instance.GetSenderConversations(userId);
        }

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

        public async Task SendChatMessage(SendChatMessageRequestDTO sendChatMessageRequest)
        {
            if (sendChatMessageRequest == null)
            {
                throw new ArgumentException("ChatRequestDTO is null");
            }
            await ConversationDAO.Instance.SendChatMessage(sendChatMessageRequest);
        }


    }
}
