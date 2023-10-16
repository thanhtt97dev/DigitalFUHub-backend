using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Cart
{
    public class ProductCartGroupResponseDTO
    {
        public long Quantity { get; set; } = 0!;
        public long ProductVariantId { get; set; }
        public string? ProductVariantName { get; set; }
        public long? Price { get; set; }
        public long? PriceDiscount { get; set; }
        public long? ProductVariantQuantity { get; set; }
        public long ProductId { get; set; }
        public string? Thumbnail { get; set; }
        public string? ProductName { get; set; }
        public int? Discount { get; set; }
    }
}
