using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Bank
{
	public class HistoryDepositRequestDTO
	{
		public string? depositTransactionId {  get; set; }	
		public string? fromDate { get; set; }
		public string? toDate { get; set; }
	}
}
