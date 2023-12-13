using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Coupon
{
    public class CouponDetailCustomerProductResponseDTO
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Thumbnail { get; set; }
    }
}
