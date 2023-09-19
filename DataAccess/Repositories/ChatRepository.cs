using BusinessObject;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class ChatRepository : IChatRepository
    {
        public async Task<List<Message>> GetListMessage(long conversationId)
        {
            if (conversationId == 0)
            {
                throw new ArgumentException("conversationId = 0, Can not get list message");
            }

            return await ChatDAO.Instance.GetListMessage(conversationId);
        }

        public async Task<List<SenderConversation>> GetSenderConversations(long userId, int page, int limit)
        {
            if (userId == 0)
            {
                throw new ArgumentException("UserId = 0, Can not get sender conversations");
            }

            return await ChatDAO.Instance.GetSenderConversations(userId, page, limit);
        }

        public async Task SendChatMessage(SendChatMessageRequestDTO sendChatMessageRequest)
        {
            if (sendChatMessageRequest == null)
            {
                throw new ArgumentException("ChatRequestDTO is null");
            }
            await ChatDAO.Instance.SendChatMessage(sendChatMessageRequest);
        }


    }
}
