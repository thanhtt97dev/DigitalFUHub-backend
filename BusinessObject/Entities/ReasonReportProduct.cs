using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class ReasonReportProduct
	{
		[Key]
		public int ReasonReportProductId { get; set; }
		public string ViName { get; set; } = string.Empty;
		public string ViExplanation { get; set; } = string.Empty;
		public string EnName { get; set; } = string.Empty;
		public string EnExplanation { get; set; } = string.Empty;
		public ICollection<ReportProduct>? ReportProducts { get; set; }
	}
}
