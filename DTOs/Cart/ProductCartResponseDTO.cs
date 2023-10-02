using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Cart
{
    public class ProductCartResponseDTO
    {
        public long ProductId { get; set; }
        public string? Thumbnail { get; set; }
        public string? ProductName { get; set; }
        public int? Discount { get; set; }
    }
}
