using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Seller
{
	public class SellerAddCouponRequestDTO
	{
		public long UserId { get; set; }
		public string CouponCode { get; set; } = string.Empty;
		public long Quantity { get; set; }
		public long AmountOrderCondition { get; set; }
		public long PriceDiscount { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public bool IsPublic { get; set; }
	}
}
