using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs
{
	public class BankAccountResponeDTO
	{
		public long BankId { get; set; }
		public string BankName { get; set; } = null!;
		public string CreditAccount { get; set; } = null!;
		public string CreditAccountName { get; set; } = null!;
		public DateTime UpdateAt { get; set; }
	}
}
