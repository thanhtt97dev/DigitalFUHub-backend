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
		public string ShopName { get; set; } = null!;
		public long ShopRegisterFeeId { get; set; }
		public string Avatar { get; set; } = string.Empty;
		public DateTime DateCreate { get; set; }
		public DateTime DateBan { get; set; }
		public string Note { get; set; } = string.Empty;
		public string? Description { get; set; }
		public bool IsActive { get; set; }
		[ForeignKey(nameof(ShopRegisterFeeId))]
		public virtual ShopRegisterFee ShopRegisterFee { get; set; } = null!;

		[ForeignKey(nameof(UserId))]
		public virtual User User { get; set; } = null!;
		public virtual ICollection<Product> Products { get; set; } = null!;
		public virtual ICollection<Coupon> Coupons { get; set; } = null!;
		public virtual ICollection<Order> Orders { get; set; } = null!;
		public virtual ICollection<Cart> Carts { get; set; } = null!;
	}
}
