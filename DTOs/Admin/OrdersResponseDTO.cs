using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Admin
{
	public class OrdersResponseDTO
	{
		public long OrderId { get; set; }
		public DateTime OrderDate { get; set; }
		public long TotalPayment { get; set; }
		public long CustomerId { get; set; }
		public string CustomerEmail { get; set; } = string.Empty;
		public long SellerId { get; set; }
		public string ShopName { get; set; } = string.Empty;
		public long OrderStatusId { get; set; }
	}
}
