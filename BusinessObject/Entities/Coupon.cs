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
        public long CouponId { get; set; }
        public string? CouponCode { get; set; }
        public long ProductId { get; set; }
        public long Quantity { get; set; }
        public long PriceDiscount { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; } = null!;
    }
}
