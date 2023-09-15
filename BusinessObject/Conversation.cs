using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class Conversation
    {
        [Key]
        public long ConversationId { get; set; }
        public DateTime DateCreate { get; set; }
        public bool isActivate { get; set; }

        public virtual ICollection<UserConversation>? UserConversations { get; set; }
        public virtual ICollection<Message>? Messages { get; set; }
    }
}
