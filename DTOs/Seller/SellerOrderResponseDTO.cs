using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		public long BusinessFee { get; set; }
		public long Profit { get; set; }
		public bool IsFeedback { get; set; }
		public string CouponCode { get; set; } = string.Empty;
		public string Username { get; set; } = string.Empty;
		public long OrderStatusId { get; set; }
	}
	public class SellerReportOrderResponseDTO
	{
		[Description("Mã đơn hàng")]
		public long OrderId { get; set; }
		[Description("Tài khoản người mua")]
		public string Username { get; set; } = string.Empty;
		[Description("Ngày mua hàng")]
		public string OrderDate { get; set; } = string.Empty;
		[Description("Tổng số tiền đơn hàng (đồng)")]
		public string TotalAmount { get; set; } = string.Empty;
		[Description("Áp dụng mã giảm giá (đồng)")]
		public string TotalCouponDiscount { get; set; } = string.Empty;
		[Description("Phí dịch vụ (%)")]
		public string BusinessFee { get; set; } = string.Empty;
		[Description("Lợi nhuận (đồng)")]
		public string Profit { get; set; } = string.Empty;
		[Description("Trạng thái đơn hàng")]
		public string OrderStatus { get; set; } = string.Empty;
	}
}
