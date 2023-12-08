using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.ReportProduct
{
	public class GetReportProductRequestDTO
	{
		public string Email { get; set; } = string.Empty;
		public string ProductId { get; set; } = string.Empty;
		public string ProductName { get; set; } = string.Empty;
		public string ShopName { get; set; } = string.Empty;
		public string FromDate { get; set; } = null!;
		public string ToDate { get; set; } = null!;
		public int ReasonReportProductId { get; set; }
		public int ReportProductStatusId { get; set; }
		public int Page { get; set; }
	}
}
