using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Conversation
{
    public class UserConversationResponseDTO
    {
        public long UserId { get; set; }
        public long RoleId { get; set; }
        public string Fullname { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
    }
}
