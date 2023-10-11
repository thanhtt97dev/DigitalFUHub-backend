using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Admin
{
	public class BusinessFeeResponseDTO
	{
		public long BusinessFeeId { get; set; }
		public long Fee { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public int TotalOrderUsed { get; set; }	
	}
}
