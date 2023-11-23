using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.TransactionCoin
{
	public class HistoryTransactionCoinRequestDTO
	{
		public string OrderId { get; set; } = null!;
		public string Email { get; set; } = null!;
		public string FromDate { get; set; } = null!;
		public string ToDate { get; set; } = null!;
		public int TransactionCoinTypeId { get; set; }
		public int Page { get; set; }
	}
}
