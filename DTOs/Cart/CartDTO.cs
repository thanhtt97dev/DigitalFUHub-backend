using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Cart
{
    public class CartDTO
    {
        public long UserId { get; set; } = 0!;
<<<<<<< HEAD
		public long ShopId { get; set; } = 0!;
		public long ProductVariantId { get; set; } = 0!;
        public long Quantity { get; set; } = 0!;
=======
        public long Quantity { get; set; } = 0!;
        public string? ShopName { get; set; }
        public long ShopId { get; set; }
        public long Coin { get; set; }
        public long ProductVariantId { get; set; }
        public ProductCartResponseDTO? Product { get; set; }
        public ProductVariantCartResponseDTO? ProductVariant { get; set; }

>>>>>>> 8413a5f9ad8791ecc6441a59f62db376271b0da6
    }
}
