using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.ReportProduct
{
	public class UpdateReportProductRequestDTO
	{
		public long ReportProductId { get; set; }
		public int Status { get; set; }
		public string Note { get; set; } = string.Empty;
	}
}
