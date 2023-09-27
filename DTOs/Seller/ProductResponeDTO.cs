using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Seller
{
	public class ProductResponeDTO
	{
		public long ProductId { get; set; }
		public string? ProductName { get; set; }
		public string? Thumbnail { get; set; }
        public List<ProductVariantResponeDTO>? ProductVariants { get; set; }
	}
}
