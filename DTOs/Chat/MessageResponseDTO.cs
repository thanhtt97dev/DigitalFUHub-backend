using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Chat
{
    public class MessageResponseDTO
    {
        public long UserId { get; set; }
        public long ConversationId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime DateCreate { get; set; }
        public string MessageType { get; set; } = string.Empty;
    }
}
