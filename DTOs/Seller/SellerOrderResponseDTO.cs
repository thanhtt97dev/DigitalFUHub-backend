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
	public class SellerReportOrderResponseDTO
	{
		public long OrderId { get; set; }
		public string Username { get; set; } = string.Empty;
		public DateTime OrderDate { get; set; }
		public string TotalAmount { get; set; } = string.Empty;
		public string TotalCouponDiscount { get; set; } = string.Empty;
		public string BusinessFee { get; set; } = string.Empty;
		public string Profit { get; set; } = string.Empty;
		public string OrderStatus { get; set; } = string.Empty;
	}
}
