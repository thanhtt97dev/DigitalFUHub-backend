using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    public class Coupon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long CouponId { get; set; }
        public long ShopId { get; set; }
        public string? CouponName { get; set; }
        public long PriceDiscount { get; set; }
        public long Quantity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }

        [ForeignKey(nameof(ShopId))]
        public virtual Shop Shop { get; set; } = null!;
		public virtual List<OrderCoupon> OrderCoupons { get; set; } = null!;
	}
}
