using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class ProductMedia
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long ProductMediaId { get; set; }
		public long ProductId { get; set; }
		public string Url { get; set; } = null!;
		[ForeignKey(nameof(ProductId))]
		public Product Product { get; set; } = null!;
	}
}
