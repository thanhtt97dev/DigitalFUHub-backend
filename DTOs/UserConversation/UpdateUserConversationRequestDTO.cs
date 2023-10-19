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
        public long UserId { get; set; }

        public bool IsValid()
        {
            return UserId > 0 && ConversationId > 0 && IsRead > 0;
        }
    }
}
