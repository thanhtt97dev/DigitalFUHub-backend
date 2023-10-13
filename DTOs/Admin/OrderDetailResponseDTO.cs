using BusinessObject.Entities;

namespace DTOs.Admin
{
	public class OrderDetailResponseDTO
	{
		public long OrderId { get; set; }
		public long CustomerId { get; set; }
		public string? CustomerEmail { get; set; }
		public long ProductVariantId { get; set; }
		public string? ProductVariantName { get; set; }
		public long ProductId { get; set; }
		public string? ProductName { get; set; }
		public string? ProductThumbnail { get; set; }
		public long ShopId { get; set; }
		public string? ShopName { get; set; }
		public long ProductCategoryId { get; set; }
		public string? ProductCategoryName { get; set; }
		public long BusinessFeeId { get; set; }
		public long BusinessFeeValue { get; set; }
		public int Quantity { get; set; }
		public long Price { get; set; }
		public long Discount { get; set; }
		public DateTime OrderDate { get; set; }
		public long TotalAmount { get; set; }
		public long TotalCouponDiscount { get; set; }
		public long TotalCoinDiscount { get; set; }	
		public long TotalPayment { get; set; }
		public string? Note { get; set; }
		public long? FeedbackId { get; set; }
		public int? FeedbackRate { get; set; }
		public long OrderStatusId { get; set; }
		/*
		public List<string>? AssetInformations { get; set; }
		public List<OrderDetailOrderCouponsDTO>? OrderCoupons { get; set; }
		*/
		public List<string>? ProductMedias { get; set; }
	}
	public class OrderDetailOrderCouponsDTO
	{
		public long PriceDiscount { get; set; }
	}
}

