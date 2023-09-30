using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Bank
{
	public class WithdrawTransactionBillRequestDTO
	{
		public long UserId { get; set; }
		public long WithdrawTransactionId { get; set;}
	}
}
