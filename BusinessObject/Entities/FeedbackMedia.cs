using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class FeedbackMedia
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long FeedbackMediaId { get; set; }
		public long FeedbackId { get; set; }
		public string Url { get; set; } = null!;
		[ForeignKey(nameof(FeedbackId))]
		public Feedback Feedback { get; set; } = null!;
	}
}
