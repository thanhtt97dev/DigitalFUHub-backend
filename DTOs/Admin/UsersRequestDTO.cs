using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Admin
{
	public class UsersRequestDTO
	{
		public string? UserId { get; set; }
		public string? Email { get; set; }
		public string? FullName { get; set; }
		public int RoleId { get; set; }
		public int Status { get; set; }
	}
}
