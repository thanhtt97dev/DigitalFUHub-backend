using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Cart
{
	public class UserCartResponseDTO
	{
		public long CartId { get; set; }
		public long ShopId { get; set; }	
		public string ShopName { get; set; } = string.Empty;
		public bool ShopActivate { get; set; }
		public ICollection<UserCartProductResponseDTO> Products { get; set; } = null!;
	}

	public class UserCartProductResponseDTO
	{
		public long CartDetailId { get; set; }
		public long ProductVariantId { get; set; }
		public string ProductVariantName { get; set; } = string.Empty;
		public long ProductVariantPrice { get; set; }
		public bool ProductVariantActivate { get; set; }
		public long ProductId { get; set; }
		public string ProductName { get; set; } = string.Empty;
		public int ProductVariantDiscount { get; set; }
		public string? ProductThumbnail { get; set; }
		public bool ProductActivate { get; set; }
		public int Quantity { get; set; }
		public int QuantityProductRemaining { get; set; }
	}
}
