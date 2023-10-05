using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Cart
{
    public class CartDTO
    {
        public long UserId { get; set; }
        public long ProductVariantId { get; set; }
        public long Quantity { get; set; }
        public string? ShopName { get; set; }
        public ProductCartResponseDTO? Product { get; set; }
        public ProductVariantCartResponseDTO? ProductVariant { get; set; }

    }
}
