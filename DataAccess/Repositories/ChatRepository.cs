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
        public async Task<List<SenderConversation>> GetSenderConversations(long userId)
        {
            if (userId == 0)
            {
                throw new ArgumentException("UserId = 0, Can not get sender conversations");
            }

            return await ChatDAO.Instance.GetSenderConversations(userId);
        }

        public async Task SendChatMessage(ChatRequestDTO chatRequestDTO)
        {
            if (chatRequestDTO == null)
            {
                throw new ArgumentException("ChatRequestDTO is null");
            }
            await ChatDAO.Instance.SendChatMessage(chatRequestDTO);
        }
    }
}
