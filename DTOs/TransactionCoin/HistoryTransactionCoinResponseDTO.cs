using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.TransactionCoin
{
	public class HistoryTransactionCoinResponseDTO
	{
		public string OrderId { get; set; } = null!;
		public long UserId { get; set; }
		public string Email { get; set; } = null!;
		public long Coin { get; set; }
		public int TransactionInternalTypeId { get; set; }
		public DateTime? DateCreate { get; set; }
	}
}
