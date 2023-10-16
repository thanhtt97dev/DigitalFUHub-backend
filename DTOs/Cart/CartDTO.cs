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
		public long ShopId { get; set; } = 0!;
		public long ProductVariantId { get; set; } = 0!;
        public long Quantity { get; set; } = 0!;
    }
}
