using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Seller
{
	public class SellerOrdersRequestDTO
	{
		public long UserId { get; set; }
		public string OrderId { get; set; } = string.Empty;
		public string CustomerEmail { get; set; } = string.Empty;
		public string FromDate { get; set; } = null!;
		public string ToDate { get; set; } = null!;
		public int Status { get; set; }
	}
}
