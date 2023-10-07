using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Coupon
{
    public class CouponProductDetailResponseDTO
    {
        public long CouponId { get; set; }
        public long ShopId { get; set; }
        public string? CouponName { get; set; }
        public long PriceDiscount { get; set; }
        public long Quantity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}
