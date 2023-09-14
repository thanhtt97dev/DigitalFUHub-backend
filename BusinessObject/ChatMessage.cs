using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class ChatMessage
    {
        public long UserId { get; set; }
        public string? Title { get; set; }
        public string? MessageContent { get; set; }
        public string? Link { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsReaded { get; set; }
    }
}
