using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
	public class OrderDetail
	{
		public long ProductId { get; set; }
		public long OrderId { get; set; }

		public long Quantity { get; set; }

		public long Price { get; set; }

		public int Discount { get; set; }

		[ForeignKey(nameof(ProductId))]
		public virtual Product Product { get; set; } = null!;
		[ForeignKey(nameof(OrderId))]
		public virtual Order Order { get; set; } = null!;

	}
}
