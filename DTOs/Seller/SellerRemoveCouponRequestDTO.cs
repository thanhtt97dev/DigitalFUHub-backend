using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Seller
{
	public class SellerRemoveCouponRequestDTO
	{
		public long UserId { get; set; }
		public long CouponId { get; set; }
	}
}
