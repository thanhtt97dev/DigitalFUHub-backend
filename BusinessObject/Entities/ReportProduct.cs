using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class ReportProduct
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long ReportProductId { get; set; }
		public long UserId { get; set; }
		public long ProductId { get; set; }
		public int ReasonReportProductId { get; set; }
		public string Description { get; set; } = string.Empty;
		public DateTime DateCreate { get; set; }
		public string Note { get; set; } = string.Empty;
		public int ReportProductStatusId { get; set; }
		[ForeignKey(nameof(UserId))]
		public virtual User User { get; set; } = null!;
		[ForeignKey(nameof(ProductId))]
		public virtual Product Product { get; set; } = null!;
		[ForeignKey(nameof(ReasonReportProductId))]
		public virtual ReasonReportProduct ReasonReportProduct { get; set; } = null!;
		[ForeignKey(nameof(ReportProductStatusId))]
		public virtual ReportProductStatus ReportProductStatus { get; set; } = null!;

	}
}
