using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Cart
{
    public class UpdateCartRequestDTO
    {
        public long UserId { get; set; }
        public long ProductVariantId { get; set; }
        public long Quantity { get; set; } = 0;
    }
}
