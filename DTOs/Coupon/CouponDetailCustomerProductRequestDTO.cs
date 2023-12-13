using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Coupon
{
    public class CouponDetailCustomerProductRequestDTO
    {
        public long CouponId { get; set; }
        public string ProductName { get; set; } = string.Empty;
    }
}
