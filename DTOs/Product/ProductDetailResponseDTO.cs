using BusinessObject.Entities;
using DTOs.Seller;
using DTOs.Shop;
using DTOs.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Product
{
    public class ProductDetailResponseDTO
    {
        public long ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Thumbnail { get; set; }
        public string? Description { get; set; }
		public long CategoryId { get; set; }
        public long Quantity { get; set; } = 0;
        public long TotalRatingStar { get; set; }
        public long NumberFeedback { get; set; }
        public long ProductStatusId { get; set; }
        public List<string>? ProductMedias { get; set; }
        public ProductDetailShopResponseDTO Shop { get; set; } = null!;
        public List<ProductDetailVariantResponeDTO>? ProductVariants { get; set; }
        public List<TagResponseDTO>? Tags { get; set; }
    }




}
