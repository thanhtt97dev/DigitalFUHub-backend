using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
	public class Feedback
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public long FeedbackId { get; set; }
		public long ProductId { get; set; }
		public long BuyerId { get; set; }
		public string? FeedbackContent { get; set; }
		public int Rate { get; set; }
		[ForeignKey(nameof(ProductId))]
		public virtual Product Product { get; set; } = null!;
		[ForeignKey(nameof(BuyerId))]
		public virtual User Buyer { get; set; } = null!;
	}
}
