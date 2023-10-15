using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Seller
{
	public class SellerUpdateStatusCouponDTO
	{
        public long UserId { get; set; }
        public long CouponId { get; set; }
        public bool IsPublic { get; set; }
    }
}
