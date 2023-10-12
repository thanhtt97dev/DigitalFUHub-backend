using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Admin
{
	public class OrderCouponResponseDTO
	{
		public long PriceDiscount { get; set; }
		public string? CouponName { get; set; }
	}
}
