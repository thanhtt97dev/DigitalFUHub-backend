using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Conversation
{
    public class MessageConversationResponseDTO
    {
        public long MessageId { get; set; }
        public long UserId { get; set; }
        public long RoleId { get; set; }
        public long ConversationId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = string.Empty;
        public DateTime DateCreate { get; set; }
    }
}
