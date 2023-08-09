using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs
{
	public class UserSignInResponseDTO
	{
		public long UserId { get; set; }
		public string? RoleName { get; set; }
		public string? Email { get; set; }
		public string? AccessToken { get; set; }
		public string? RefreshToken { get; set; }
		public string? JwtId { get; set; }
	}
}
