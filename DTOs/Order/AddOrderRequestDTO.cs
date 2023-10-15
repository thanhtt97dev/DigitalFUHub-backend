using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Order
{
	public class AddOrderRequestDTO
	{
		public long UserId { get; set; }
		public List<ShopProductRequestAddOrderDTO> ShopProducts { get; set; } = new List<ShopProductRequestAddOrderDTO>();
		public bool IsUseCoin { get; set; }
	}

	public class ShopProductRequestAddOrderDTO
	{
		public long ShopId { get; set; }
		public List<ProductRequestAddOrderDTO> Products { get; set; } = new List<ProductRequestAddOrderDTO>();
		public string Coupon { get; set; } = string.Empty;
	}

	public class ProductRequestAddOrderDTO 
	{
		public long ProductVariantId { get; set; }
		public int Quantity { get; set; }
	}
}
