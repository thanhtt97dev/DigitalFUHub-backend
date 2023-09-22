using BusinessObject.DataTransfer;
using BusinessObject.Entities;
using DTOs.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
    public interface IChatRepository
    {
        Task<List<SenderConversation>> GetSenderConversations(long userId);

        Task SendChatMessage(SendChatMessageRequestDTO sendChatMessageRequest);

        Task<List<Message>> GetListMessage(long conversationId);
    }
}
