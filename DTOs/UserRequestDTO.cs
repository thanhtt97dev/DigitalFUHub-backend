using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs
{
	public class UserRequestDTO
	{
		public long? RoleId { get; set; }
		public int? Status { get; set; }
		public string Email { get; set; } = null!;
	}
}
