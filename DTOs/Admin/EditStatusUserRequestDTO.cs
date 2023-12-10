using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Admin
{
	public class EditStatusUserRequestDTO
	{
		public long UserId { get; set; }
		public bool Status { get; set; }
		public string Note { get; set; } = string.Empty;
	}
}
