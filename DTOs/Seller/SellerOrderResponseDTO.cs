using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Seller
{
	public class SellerOrderResponseDTO
	{
		public long OrderId { get; set; }
		public DateTime OrderDate { get; set; }
		public long TotalAmount { get; set; }
		public long TotalCouponDiscount { get; set; }
		public string Username { get; set; } = string.Empty;
		public long OrderStatusId { get; set; }
	}
}
