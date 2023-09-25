using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities2
{
	public class ProductStatus
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long ProductStatusId { get; set; }
		public long ProductId { get; set; }
		public string? ProductStatusName { get; set; }

		[ForeignKey(nameof(ProductId))]
		public virtual ICollection< Product>? Products { get; set; }
	}
}
