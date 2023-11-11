using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class ReportProductStatus
	{
		[Key]
		public int ReportProductId { get; set; }
		public string Name { get; set; } = string.Empty;
		public ICollection<ReportProduct>? ReportProducts { get; set; }

	}
}
