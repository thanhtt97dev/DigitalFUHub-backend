using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Order
{
	public class OrderResponseDTO
	{
		public int NextOffset { get; set; }
		public List<OrderProductResponseDTO> Orders { get; set; } = new();
	}

	public class OrderProductResponseDTO
	{
		public long OrderId { get; set; }
		public long ShopId { get; set; }
		public string ShopName { get; set; } = string.Empty;
		public long? ConversationId { get; set; }
		public DateTime OrderDate { get; set; }
		public DateTime? DateConfirmed { get; set; } = null!;
		public string Note { get; set; } = string.Empty;
		public long TotalAmount { get; set; }
		public long TotalCouponDiscount { get; set; }
		public long TotalCoinDiscount { get; set; }
		public long TotalPayment { get; set; }
		public long StatusId { get; set; }
		public List<OrderDetailProductResponseDTO> OrderDetails { get; set; } = null!;
		public List<HistoryOrderStatusResponseDTO> HistoryOrderStatus { get; set; } = null!;
	}
	public class OrderDetailProductResponseDTO
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
		public List<string> AssetInformations { get; set; } = null!;
		public int FeedbackRate { get; set; }
		public bool IsFeedback { get; set; }
	}
	public class HistoryOrderStatusResponseDTO
	{
		public long OrderId { get; set; }
		public long OrderStatusId { get; set; }
		public DateTime DateCreate { get; set; }
		public string Note { get; set; } = string.Empty;

	}
}
