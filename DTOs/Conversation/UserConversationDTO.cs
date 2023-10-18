using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Conversation
{
	public class UserConversationDTO
	{
		public long UserId { get; set; }
		public long ConversationId { get; set; }
		public bool IsGroup { get; set; }
		public List<long> MembersInGroup { get; set; } = new List<long>();
	}
}
