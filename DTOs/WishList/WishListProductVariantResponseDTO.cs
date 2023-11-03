using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.WishList
{
    public class WishListProductVariantResponseDTO
    {
        public long ProductVariantId { get; set; }
        public long Price { get; set; }
    }
}
