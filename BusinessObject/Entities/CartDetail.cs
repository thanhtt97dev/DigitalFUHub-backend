using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class CartDetail
	{
		public long CartDetailId { get; set; }
		public long CartId { get; set; }
		public long ProductVariantId { get; set; }
		public int Quantity { get; set; }

		[ForeignKey(nameof(CartId))]
		public virtual Cart Cart { get; set; } = null!;

		[ForeignKey(nameof(ProductVariantId))]
		public virtual ProductVariant ProductVariant { get; set; } = null!;

	}
}
