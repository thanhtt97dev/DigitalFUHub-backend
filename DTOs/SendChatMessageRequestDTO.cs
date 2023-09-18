using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs
{
    public class SendChatMessageRequestDTO
    {
        public long ConversationId { get; set; } = 0;
        public long SenderId { get; set; }
        public long RecipientId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime DateCreate { get; set; }
    }
}
