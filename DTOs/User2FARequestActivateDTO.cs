using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs
{
	public class User2FARequestActivateDTO
	{
		public string SecretKey { get; set; } = null!;
		public string Code { get; set; } = null!;
	}
}
