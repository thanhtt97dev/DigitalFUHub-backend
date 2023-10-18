using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.User
{
	public class UserOnlineStatusHubDTO
	{
		public long ConversationId { get; set; }
		public bool IsOnline { get; set; }
	}
}
