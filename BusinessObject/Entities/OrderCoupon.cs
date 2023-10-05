using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class OrderCoupon
	{
		public long OrderId { get; set; }
		public long CouponId { get; set; }
		public DateTime UseDate { get; set; }

		[ForeignKey(nameof(OrderId))]
		public virtual Order? Order { get; set; }
		[ForeignKey(nameof(CouponId))]
		public virtual Coupon Coupon { get; set; } = null!;
	}
}
