using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Admin
{
	public class UsersResponseDTO
	{
		public long UserId { get; set; }
		public long RoleId { get; set; }
		public string? RoleName { get; set; }
		public string? Email { get; set; }
		public string? Fullname { get; set; }
		public string? Avatar { get; set; }
		public string? Status { get; set; }
	}
}
