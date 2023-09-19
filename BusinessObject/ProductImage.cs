using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
	public class ProductImage
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public long ProductImageId { get; set; }
		public long ProductId { get;set; }
		public string? ProductImageName { get; set; }
		[ForeignKey(nameof(ProductId))]
		public virtual Product Product { get; set; } = null!;
	}
}
