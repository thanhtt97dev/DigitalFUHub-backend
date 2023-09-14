using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs
{
	public class DepositTransactionRequestDTO
	{
		public long UserId { get; set; }
		public long Amount { get; set; }
	}
}
