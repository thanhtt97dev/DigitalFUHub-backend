using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.WishList
{
    public class WishListProductResponseDTO
    {
        public long ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Discount { get; set; }
        public string? Thumbnail { get; set; }
        public long ProductStatusId { get; set; }
        public List<WishListProductVariantResponseDTO>? ProductVariants { get; set; }
    }
}
