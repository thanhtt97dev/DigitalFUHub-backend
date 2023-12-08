using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.ReportProduct
{
	public class GetReportProductResponseDTO
	{
		public long ReportProductId { get; set; }
		public long CustomerId { get; set; }
		public string CustomerEmail { get; set; } = string.Empty;
		public long ProductId { get; set; }
		public string ProductName { get; set; } = string.Empty;
		public string ProductThumbnail { get; set; } = string.Empty;
		public long ShopId { get; set; }
		public string ShoptName { get; set; } = string.Empty;
		public DateTime DateCreate { get; set; }
		public string Description { get; set; } = string.Empty;
		public string Note { get; set; } = string.Empty;
		public int ReasonReportProductId { get; set; }
		public string ReasonReportProductViName { get; set; } = string.Empty;
		public string ReasonReportProductViExplanation { get; set; } = string.Empty;
		public int ReportProductStatusId { get; set; }
	}
}
