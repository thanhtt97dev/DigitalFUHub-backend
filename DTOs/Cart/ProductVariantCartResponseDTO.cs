using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Cart
{
    public class ProductVariantCartResponseDTO
    {
        public string? ProductVariantName { get; set; }
        public long? Price { get; set; }
        public long? PriceDiscount { get; set; }
        public long? Quantity { get; set; }
    }
}
