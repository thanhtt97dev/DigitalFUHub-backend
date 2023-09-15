using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class UserConversation
    {
        [Key]
        public long UserConversationId { get; set; }
        public long UserId { get; set; }
        public long ConversationId { get; set; }

        [ForeignKey(nameof(ConversationId))]
        public virtual Conversation Conversation { get; set; }= null!;

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
    }
}
