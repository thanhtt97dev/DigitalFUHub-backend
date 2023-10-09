using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Admin
{
	public class RejectWithdrawTransaction
	{
		public long WithdrawTransactionId { get; set; }	
		public string? Note { get; set; }
	}
}
