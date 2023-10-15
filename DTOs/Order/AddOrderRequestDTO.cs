﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Order
{
	public class AddOrderRequestDTO
	{
		public long UserId { get; set; }
		public List<ProductRequestAddOrderDTO> Products { get; set; } = new List<ProductRequestAddOrderDTO>();
		public bool IsUseCoin { get; set; }	
	}

	public class ProductRequestAddOrderDTO 
	{
		public long ProductVariantId { get; set; }
		public int Quantity { get; set; }
		public string Coupon { get; set; } = string.Empty;
	}

	public class CouponRequestAddOrderDTO
	{
		public string CouponCode { get; set; } = string.Empty;	
	}
}
