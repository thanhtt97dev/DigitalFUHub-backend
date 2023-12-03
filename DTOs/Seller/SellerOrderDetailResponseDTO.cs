using DTOs.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Seller
{
	public class SellerOrderDetailResponseDTO
	{
		public long OrderId { get; set; }
		public long ShopId { get; set; }
		public long CustomerId { get; set; }
		public long? ConversationId { get; set; } = null!;
		public string CustomerUsername { get; set; } = string.Empty;
		public string CustomerAvatar { get; set; } = string.Empty;
		public DateTime OrderDate { get; set; }
		public string Note { get; set; } = string.Empty;
		public long TotalAmount { get; set; }
		public long TotalCouponDiscount { get; set; }
		//public long TotalCoinDiscount { get; set; }
		//public long TotalPayment { get; set; }
		public string CouponCode { get; set; } = string.Empty;
		public long PercentBusinessFee { get; set; }
		public long BusinessFeePrice { get; set; }
		public long AmountSellerReceive { get; set; }
		public long StatusId { get; set; }
		public List<SellerHistoryOrderResponseDTO> HistoryOrderStatus { get; set; } = null!;
		public List<SellerOrderDetailProductResponseDTO> OrderDetails { get; set; } = null!;
	}
	public class SellerOrderDetailProductResponseDTO
	{
		public long OrderDetailId { get; set; }
		public long ProductId { get; set; }
		public long ProductVariantId { get; set; }
		public string ProductName { get; set; } = string.Empty;
		public string ProductVariantName { get; set; } = string.Empty;
		public string Thumbnail { get; set; } = string.Empty;
		public int Quantity { get; set; }
		public long Price { get; set; }
		public long Discount { get; set; }
		public long TotalAmount { get; set; }
		public int FeedbackRate { get; set; }
		public bool IsFeedback { get; set; }
	}
	public class SellerHistoryOrderResponseDTO
	{
		public long OrderId { get; set; }
		public long OrderStatusId { get; set; }
		public DateTime DateCreate { get; set; }
		public string Note { get; set; } = string.Empty;
	}
}
