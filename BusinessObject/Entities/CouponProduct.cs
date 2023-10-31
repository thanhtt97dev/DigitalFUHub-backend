using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class CouponProduct
	{
		[Key]
		public long CouponId { get; set; }
		[Key]
		public long ProductId { get; set; }	
		public DateTime UpdateDate { get; set; }	
		public bool IsActivate { get; set; }
		[ForeignKey(nameof(CouponId))]
		public virtual Coupon Coupon { get; set; } = null!;
		[ForeignKey(nameof(ProductId))]
		public virtual Product Product { get; set; } = null!;
	}
}
