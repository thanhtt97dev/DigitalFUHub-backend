using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Seller
{
	public class SellerCouponResponseDTO
	{
		public long CouponId { get; set; }
		public string CouponCode { get; set; } = string.Empty;
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public long Quantity { get; set; }
		public long PriceDiscount { get; set; }
		public long AmountOrderCondition { get; set; }
		public bool IsPublic { get; set; }
	}
}
