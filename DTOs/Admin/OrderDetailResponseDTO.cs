using BusinessObject.Entities;

namespace DTOs.Admin
{
	public class OrderDetailResponseDTO
	{
		public long OrderId { get; set; }
		public long UserId { get; set; }
		public long ProductVariantId { get; set; }
		public long BusinessFeeId { get; set; }
		public int Quantity { get; set; }
		public long Price { get; set; }
		public DateTime OrderDate { get; set; }
		public long TotalAmount { get; set; }
		public bool IsFeedback { get; set; }
		public long OrderStatusId { get; set; }
		public BusinessObject.Entities.User? Customer { get; set; }	
		public OrderStatus? OrderStatus { get; set; }
		public ProductVariant? ProductVariant { get; set; }
		public BusinessFee? BusinessFee { get; set; } = null!;
		public List<AssetInformation>? AssetInformations { get; set; }
		public List<OrderCoupon>? OrderCoupons { get; set; }
	}
}
