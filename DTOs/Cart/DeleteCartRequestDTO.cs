using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Cart
{
    public class DeleteCartRequestDTO
    {
        public long UserId { get; set; }
        public long ProductVariantId { get; set; }
    }
}
