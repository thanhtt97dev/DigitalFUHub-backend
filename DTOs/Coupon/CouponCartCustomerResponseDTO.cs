using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Coupon
{
    public class CouponCartCustomerResponseDTO
    {
        public long CouponId { get; set; }
        public string? CouponName { get; set; }
        public string CouponCode { get; set; } = string.Empty;
        public long CouponTypeId { get; set; }
        public long PriceDiscount { get; set; }
        public long Quantity { get; set; }
        public long MinTotalOrderValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<long>? productIds { get; set; }
    }
}
