using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Chat
{
    public class SendMessageConversationRequestDTO
    {
        public long ConversationId { get; set; }
        public long UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public IEnumerable<IFormFile>? Images { get; set; }
        public IEnumerable<long> RecipientIds { get; set; } = null!;
    }
}

