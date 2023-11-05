using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Conversation
{
    public class ConversationResponseDTO
    {
        public long ConversationId { get; set; }
        public string? ConversationName { get; set; }
        public DateTime DateCreate { get; set; }
        public bool IsActivate { get; set; }
        public bool IsRead { get; set; }
        public ConversationLatestMessageResponseDTO? LatestMessage { get; set; }
        public bool IsGroup { get; set; }
        public DateTime LastTimeOnline { get; set; }
        public bool IsOnline { get; set; }
        public ICollection<UserConversationResponseDTO>? Users { get; set; }

    }

    public class ConversationLatestMessageResponseDTO
    {
        public string Content { get; set; }
        public DateTime DateCreate { get; set; }   

	}
}
