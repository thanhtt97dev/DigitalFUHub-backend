using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Admin
{
	public class UsersRequestDTO
	{
		public string UserId { get; set; } = string.Empty;	
		public string Email { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public int RoleId { get; set; }
		public int Status { get; set; }
		public int Page { get; set; }
	}
}
