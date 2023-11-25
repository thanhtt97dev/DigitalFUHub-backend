using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Bank
{
	public class WithdrawTransactionUnpayResponseDTO
	{
		public long WithdrawTransactionId { get; set; }
		public long UserId { get; set; }
		public string Email { get; set; } = null!;
		public long Amount { get; set; }
		public string Code { get; set; } = null!;
		public DateTime RequestDate { get; set; }
		public DateTime? PaidDate { get; set; }
		public string CreditAccountName { get; set; } = null!;
		public string CreditAccount { get; set; } = null!;
		public string BankName { get; set; } = string.Empty;
		public string BankCode { get; set; } = null!;
		public string BankId { get; set; } = null!;
		public long WithdrawTransactionStatusId { get; set; }
	}
}
