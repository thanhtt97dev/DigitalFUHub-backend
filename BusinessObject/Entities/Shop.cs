using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class Shop
	{
		[Key]
		public long UserId { get; set; }
		[ForeignKey(nameof(UserId))]
		public virtual User User { get; set; } = null!;
		public string? ShopName { get; set; }
		public DateTime DateCreate { get; set; }
		public long Balance { get; set; }
		public string? Description { get; set; }
		public bool IsActive { get; set; }

		public virtual ICollection<Product>? Products { get; set; }
		public virtual ICollection<Coupon>? Coupons { get; set; }
	}
}
