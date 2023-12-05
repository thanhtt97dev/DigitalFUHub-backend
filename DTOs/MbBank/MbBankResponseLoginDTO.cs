using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.MbBank
{
	public class MbBankResponseLoginDTO
	{
		public string? refNo { get; set; }
		public Result result { get; set; } = null!;
		public string sessionId { get; set; } = string.Empty;
		public string cust { get; set; } = string.Empty;
	}
}
