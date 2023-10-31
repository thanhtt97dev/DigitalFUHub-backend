using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Seller
{
	public class SellerDisputeOrderRequestDTO
	{
		public long SellerId { get; set; }
		public long CustomerId { get; set; }
		public long OrderId { get; set; }
		public string Note { get; set; } = string.Empty;
	}
	public class SellerRefundOrderRequestDTO
	{
		public long SellerId { get; set; }
		public long OrderId { get; set; }
		public string Note { get; set; } = string.Empty;
	}
}
