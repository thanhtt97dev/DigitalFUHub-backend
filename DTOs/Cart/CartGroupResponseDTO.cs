using DTOs.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Cart
{
    public class CartGroupResponseDTO
    {
        public long UserId { get; set; } = 0!;
        public string? ShopName { get; set; }
        public long ShopId { get; set; }
        public long Coin { get; set; }
        public string? Coupons { get; set; }
        public IEnumerable<ProductCartGroupResponseDTO>? Products { get; set; }
    }
}

