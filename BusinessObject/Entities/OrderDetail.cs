using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class OrderDetail
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]	
		public long OrderDetailId { get; set; }
		public long OrderId { get; set; }
		public long ProductVariantId { get; set; }
		public int Quantity { get; set; }
		public long Price { get; set; }	
		public int Discount { get;set; }
		public long TotalAmount { get; set; }
		public bool IsFeedback { get; set; }

		[ForeignKey(nameof(OrderId))]
		public virtual Order Order { get; set; } = null!;
		[ForeignKey(nameof(ProductVariantId))]
		public virtual ProductVariant ProductVariant { get; set; } = null!;
		public virtual List<AssetInformation> AssetInformations { get; set; } = null!;
		public Feedback? Feedback { get; set; } = null!;
	}
}
