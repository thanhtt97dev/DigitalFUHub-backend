using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Admin
{
	public class OrdersResponseDTO
	{
		public string OrderId { get; set; } = string.Empty;
		public DateTime OrderDate { get; set; }
		public long TotalAmount { get; set; }
		public long CustomerId { get; set; }
		public string CustomerEmail { get; set; } = string.Empty;
		public long SellerId { get; set; }
		public string ShopName { get; set; } = string.Empty;
		public int OrderStatusId { get; set; }
	}
}
