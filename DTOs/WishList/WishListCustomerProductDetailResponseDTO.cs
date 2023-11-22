using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.WishList
{
    public class WishListCustomerProductDetailResponseDTO
    {
        public long ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Thumbnail { get; set; }
        public long TotalRatingStar { get; set; }
        public long NumberFeedback { get; set; }
        public int SoldCount { get; set; }
        public long ProductStatusId { get; set; }
        public int QuantityProductRemaining { get; set; }
        public WishListCustomerProductVariantResponseDTO? ProductVariant { get; set; }
    }
}
