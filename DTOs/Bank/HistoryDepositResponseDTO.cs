using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Bank
{
	public class HistoryDepositResponseDTO
	{
		public int Total { get; set; }
		public List<DepositTransaction> DepositTransactions { get; set; } = new List<DepositTransaction>();
	}
}
