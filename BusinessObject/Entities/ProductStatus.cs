using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class ProductStatus
	{
		[Key]
		public long ProductStatusId { get; set; }
		public string? ProductStatusName { get; set; }
	}
}
