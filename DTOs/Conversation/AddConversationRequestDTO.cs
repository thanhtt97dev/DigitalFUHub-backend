using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Conversation
{
    public class AddConversationRequestDTO
    {
        public string? ConversationName { get; set; }
        public DateTime DateCreate { get; set; }
        public List<long> UserIds { get; set; } = new List<long>();
    }
}
