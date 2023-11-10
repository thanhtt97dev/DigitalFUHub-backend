using BusinessObject.Entities;

namespace DTOs.Admin
{
	public class OrderInfoResponseDTO
	{
		public long OrderId { get; set; }
		public long CustomerId { get; set; }
		public string? CustomerEmail { get; set; }
		public string? CustomerAvatar { get; set; }
		public long ShopId { get; set; }
		public string? ShopName { get; set; }
		public string ShopAvatar { get; set; } = string.Empty;
		public long BusinessFeeId { get; set; }
		public long BusinessFeeValue { get; set; }
		public long ConversationId { get; set; }
		public long OrderStatusId { get; set; }
		public DateTime OrderDate { get; set; }
		public string? Note { get; set; }
		public long TotalAmount { get; set; }
		public long TotalCouponDiscount { get; set; }
		public long TotalCoinDiscount { get; set; }
		public long TotalPayment { get; set; }
		public List<OrderDetailInfoResponseDTO> OrderDetails { get; set; } = new List<OrderDetailInfoResponseDTO>();
		public List<TransactionInternalOrderDetailResponseDTO> TransactionInternals { get; set; } = new List<TransactionInternalOrderDetailResponseDTO>();
		public List<TransactionCoinOrderDetailResponseDTO> TransactionCoins { get; set; } = new List<TransactionCoinOrderDetailResponseDTO>();
		public List<HistoryOrderStatusOrderDetailDTO> HistoryOrderStatus { get; set; } = new List<HistoryOrderStatusOrderDetailDTO>();
	}
	public class OrderDetailInfoResponseDTO
	{
		public long OrderDetailId { get; set; }
		public long ProductVariantId { get; set; }
		public string ProductVariantName { get; set; } = string.Empty;
		public long ProductId { get; set; }
		public string ProductName { get; set; } = string.Empty;
		public string ProductThumbnail { get; set; } = string.Empty;
		public int Quantity { get; set; }
		public long Price { get; set; }
		public int Discount { get; set; }
		public long TotalAmount { get; set; }
		public bool IsFeedback { get; set; }
		public int FeedbackRate { get; set; }
		public long FeedbackBenefit { get; set; }
		public List<AssetInfomationOrderDetailResponeDTO> AssetInfomations { get; set; } = new List<AssetInfomationOrderDetailResponeDTO>();
		
	}

	public class AssetInfomationOrderDetailResponeDTO
	{
		public string Data { get; set; } = string.Empty;
	}

	public class TransactionInternalOrderDetailResponseDTO
	{
		public int TransactionInternalTypeId { get; set; }
		public long PaymentAmount { get; set; }
		public DateTime DateCreate { get; set; }
	}

	public class TransactionCoinOrderDetailResponseDTO
	{
		public int TransactionCoinTypeId { get; set; }
		public long Amount { get; set; }
		public DateTime DateCreate { get; set; }
	}

	public class HistoryOrderStatusOrderDetailDTO
	{
		public int OrderStatusId { get; set; }
		public DateTime DateCreate { get; set; }
		public string Note { get; set; } = string.Empty;
	}
}

