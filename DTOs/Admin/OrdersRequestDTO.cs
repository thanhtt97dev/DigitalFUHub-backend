using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Admin
{
	public class OrdersRequestDTO
	{
		public string OrderId { get; set; }	 = string.Empty;	
		public string CustomerEmail{ get; set; } = string.Empty;
		public string ShopId { get; set; } = string.Empty;
		public string ShopName { get; set; } = string.Empty;	
		public string FromDate { get; set; } = null!;
		public string ToDate { get; set; } = null!;
		public int Status { get; set; }
	}
}
