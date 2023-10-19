using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.UserConversation
{
    public class UpdateUserConversationRequestDTO
    {
        public long ConversationId { get; set; }
        public int IsRead { get; set; }
        public List<long> RecipientIds { get; set; } = new List<long>();

        public bool IsValid()
        {
            return (RecipientIds != null && RecipientIds.Count > 0) && ConversationId > 0 && IsRead > 0;
        }
    }
}
