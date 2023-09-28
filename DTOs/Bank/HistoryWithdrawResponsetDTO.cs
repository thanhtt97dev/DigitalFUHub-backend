using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Bank
{
	public class HistoryWithdrawResponsetDTO
	{
		public long WithdrawTransactionId { get; set; }
		public long Amount { get; set; }
		public DateTime RequestDate { get; set; }
		public string CreditAccountName { get; set; } = null!;
		public string CreditAccount { get; set; } = null!;
		public string BankName { get; set; } = string.Empty;
		public bool IsPay { get; set; }

	}
}
