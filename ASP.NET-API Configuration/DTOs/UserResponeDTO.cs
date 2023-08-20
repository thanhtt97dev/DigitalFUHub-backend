using BusinessObject;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs
{
	public class UserResponeDTO
	{
		public long UserId { get; set; }
		public long RoleId { get; set; }
		public string? RoleName { get; set; }	
		public string? Email { get; set; }
		public bool? Status { get; set; }
	}
}
