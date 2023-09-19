using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
	public class ProductWarehouse
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long ProductWarehouseId { get; set; }
		public long ProductId { get; set; }

		public string? ProductWarehouseName { get; set; }
		public bool IsSold { get; set; }

		[ForeignKey(nameof(ProductId))]
		public virtual Product Product { get; set; } = null!;
	}
}
