using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Seller
{
	public class SellerProductResponseDTO
	{
		public long ProductId { get; set; }
		public string? ProductName { get; set; }
		public string? Thumbnail { get; set; }
        public List<ProductDetailVariantResponeDTO>? ProductVariants { get; set; }
	}
}
