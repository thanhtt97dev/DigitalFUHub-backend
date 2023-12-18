using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.ShopRegisterFee
{
	public class ShopRegisterFeeResponseDTO
	{
		public long ShopRegisterFeeId { get; set; }
		public long Fee { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public int TotalShopUsed { get; set; }
	}
}
