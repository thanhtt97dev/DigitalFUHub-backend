using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Seller
{
	public class SellerCouponRequestDTO
	{
		public long UserId { get; set; } 
		public string CouponCode { get; set; } = string.Empty;
		public string? StartDate { get; set; } = null!;
		public string? EndDate { get; set; } = null!;
		public bool? Status { get; set; }
	}
}
