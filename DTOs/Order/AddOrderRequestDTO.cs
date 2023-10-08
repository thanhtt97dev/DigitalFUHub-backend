using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Order
{
	public class AddOrderRequestDTO
	{
		public long UserId { get; set; }
		public long ProductVariantId { get; set; }
		public long BusinessFeeId { get; set; }
		public int Quantity { get; set; }
		public List<string> Coupons { get; set; } = new List<string>();
	}

	public class CouponRequestAddOrderDTO
	{
		public string CouponCode { get; set; } = string.Empty;	
	}
}
