using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Statistic
{
	public class StatisticSalesRequestDTO
	{
		public int Month { get; set; }
		public int Year { get; set; }
		public int StatusOrder { get; set; }
	}
	public class StatisticDepositAndWithdrawnRequestDTO
	{
		public int Month { get; set; }
		public int Year { get; set; }
	}
}
