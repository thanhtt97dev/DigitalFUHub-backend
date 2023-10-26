using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Conversation
{
	public class GetConversationIdRequestDTO
	{
		public long ShopId { get; set; }	
		public long UserId { get; set; }
		public bool IsGroup { get; set; }
	}
}
