using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs
{
	public class UserSignInRequestDTO
	{
		public string Username { get; set; } = string.Empty;
		public string? Password { get; set; }
	}
}
