using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class CouponType
	{
		[Key]
		public long CouponTypeId { get; set; }	
		public string Name { get; set; } = string.Empty;
		public virtual ICollection<Coupon>? Coupons { get; set; }
	}
}
