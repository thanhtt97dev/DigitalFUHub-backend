using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Seller
{
	public class SellerAddCouponRequestDTO
	{
		public long UserId { get; set; }
		public string CouponName { get; set; } = string.Empty;
		public string CouponCode { get; set; } = string.Empty;
		public long Quantity { get; set; }
		public long MinTotalOrderValue { get; set; }
		public long PriceDiscount { get; set; }
		public string StartDate { get; set; } = string.Empty;
		public string EndDate { get; set; } = string.Empty;
		public bool IsPublic { get; set; }
		public long TypeId { get; set; }
		public List<long> ProductsApplied { get; set; } = new(); 
	}
	public class SellerEditCouponRequestDTO
	{
		public long UserId { get; set; }
		public long CouponId { get; set; }
		public string CouponName { get; set; } = string.Empty;
		public string CouponCode { get; set; } = string.Empty;
		public long Quantity { get; set; }
		public long MinTotalOrderValue { get; set; }
		public long PriceDiscount { get; set; }
		public string StartDate { get; set; } = string.Empty;
		public string EndDate { get; set; } = string.Empty;
		public bool IsPublic { get; set; }
	}
}
