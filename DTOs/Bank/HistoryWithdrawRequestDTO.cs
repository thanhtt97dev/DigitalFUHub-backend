using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Bank
{
	public class HistoryWithdrawRequestDTO
	{
		public string? WithdrawTransactionId { get; set; }
		public string? Email { get; set; }
		public string? FromDate { get; set; }
		public string? ToDate { get; set; }
		public long BankId { get; set; }
		public string? CreditAccount { get; set; }
		public int Status { get; set; }
	}
}
