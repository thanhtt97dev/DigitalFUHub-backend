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
		public long UserId { get; set; }
		public long ProductVariantId { get; set; }
		public long Quantity { get; set; }

		[ForeignKey(nameof(ProductVariantId))]
		public virtual ProductVariant ProductVariant { get; set; } = null!;
		[ForeignKey(nameof(UserId))]
		public virtual User User { get; set; } = null!;
	}
}
