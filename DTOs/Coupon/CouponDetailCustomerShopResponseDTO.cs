using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Coupon
{
    public class CouponDetailCustomerShopResponseDTO
    {
        public long UserId { get; set; }
        public string ShopName { get; set; } = null!;
        public string Avatar { get; set; } = string.Empty;
    }
}
