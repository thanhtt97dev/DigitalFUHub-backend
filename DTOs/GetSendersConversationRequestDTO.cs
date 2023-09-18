using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs
{
    public class GetSendersConversationRequestDTO
    {
        public long UserId { get; set; }
        public int page { get; set; }
        public int limit { get; set; }
    }
}
