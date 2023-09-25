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
		public long ProductTypeId { get; set; }
		public long Quantity { get; set; }

		[ForeignKey(nameof(ProductTypeId))]
		public virtual ProductType? ProductType { get; set; }
		[ForeignKey(nameof(UserId))]
		public virtual User? User { get; set; }
	}
}
