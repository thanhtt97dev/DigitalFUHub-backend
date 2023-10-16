using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class Cart
	{
		public long CartId { get; set; }
		public long UserId { get; set; }
		public long ShopId { get; set; }
		public long Quantity { get; set; }

		[ForeignKey(nameof(UserId))]
		public virtual User User { get; set; } = null!;
		[ForeignKey(nameof(ShopId))]
		public virtual Shop Shop { get; set; } = null!;
		public virtual ICollection<CartDetail> CartDetails { get; set; } = null!;
	}
}
