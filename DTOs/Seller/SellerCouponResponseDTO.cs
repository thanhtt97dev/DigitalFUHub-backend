using BusinessObject.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Seller
{
	public class ListCouponResponseDTO
	{
		public long TotalItems { get; set; }
		public List<SellerCouponResponseDTO> Coupons { get; set; } = new();
	}
	public class SellerCouponResponseDTO
	{
		public long CouponId { get; set; }
		public string CouponName { get; set; } = string.Empty;
		public string CouponCode { get; set; } = string.Empty;
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public long Quantity { get; set; }
		public long PriceDiscount { get; set; }
		public long MinTotalOrderValue { get; set; }
		public bool IsPublic { get; set; }
		public long CouponTypeId { get; set; }
		public List<BusinessObject.Entities.Product> ProductsApplied { get; set; } = new();
	}
}
