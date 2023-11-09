using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Conversation
{
    public class SendMessageConversationRequestDTO
    {
        public long ConversationId { get; set; }
        public long UserId { get; set; }
        public string? Content { get; set; }
        public IFormFile? Image { get; set; }
        public IEnumerable<long> RecipientIds { get; set; } = null!;
    }
}

