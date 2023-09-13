using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs
{
	public class MbBankResponeHistoryTransactionDataDTO
	{
		public string? refNo { get; set; }
		public Result result { get; set; } = null!;
		public List<TransactionHistory>? transactionHistoryList { get; set; }
	}

	public class Result
	{
		public string? responseCode { get; set; }
		public string? message { get; set; }
		public bool ok { get; set; }
	}

	public class TransactionHistory
	{
		public DateTime? postingDate { get; set; }
		public DateTime? transactionDate { get; set; }
		public string? accountNo { get; set; }
		public int creditAmount { get; set; }
		public int debitAmount { get; set; }
		public string? currency { get; set; }
		public string description { get; set; } = null!;
		public int availableBalance { get; set; }
		public string? beneficiaryAccount { get; set; }
		public string refNo { get; set; } = null!;
		public string? benAccountName { get; set; }
		public string? bankName { get; set; }
		public string? benAccountNo { get; set; }
		public DateTime? dueDate { get; set; }
		public string? docId { get; set; }
		public string? transactionType { get; set; }

	}
}
