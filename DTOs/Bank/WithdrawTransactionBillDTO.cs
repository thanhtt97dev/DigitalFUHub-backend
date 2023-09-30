using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Bank
{
	public class WithdrawTransactionBillDTO
	{
		public long WithdrawTransactionId { get; set; }
		public DateTime? PostingDate { get; set; }
		public int DebitAmount { get; set; }
		public string Description { get; set; } = null!;
		public string RefNo { get; set; } = null!;
		public string? BenAccountName { get; set; }
		public string? BankName { get; set; }
		public string? BenAccountNo { get; set; }
	}
}
