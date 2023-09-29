using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Bank
{
	public class HistoryDepositForAdminRequestDTO
	{
		public string? DepositTransactionId { get; set; }
		public string? FromDate { get; set; }
		public string? ToDate { get; set; }
	}
}
