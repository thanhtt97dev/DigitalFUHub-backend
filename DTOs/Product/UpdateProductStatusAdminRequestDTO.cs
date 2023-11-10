using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Product
{
	public class UpdateProductStatusAdminRequestDTO
	{
		public long ProductId { get; set; }
		public int Status { get; set; }
		public string Note { get; set; } = string.Empty;
	}
}
