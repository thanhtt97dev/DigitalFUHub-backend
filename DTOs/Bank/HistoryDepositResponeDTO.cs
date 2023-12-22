using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Bank
{
	public class HistoryDepositResponeDTO
	{
		public long DepositTransactionId { get; set; }
		public long UserId { get; set; }
		public string Email { get; set; } = null!;
		public DateTime? RequestDate { get; set; }
		public DateTime? PaidDate { get; set; }
		public string Code { get; set; } = null!;
		public long Amount { get; set; }
		public int DepositTransactionStatusId { get; set; }
	}
}
