using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Seller
{
	public class ProductDetailVariantResponeDTO
	{
		public long ProductVariantId { get; set; }
		public string? Name { get; set; }
		public long? Price { get; set; }
        public int Discount { get; set; }
        public long? Quantity { get; set; }
	}
}
