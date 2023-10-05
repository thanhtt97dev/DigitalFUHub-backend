using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.Tag;
using DTOs.Seller;

namespace DTOs.Product
{
	public class ProductResponseDTO
	{
		public long ProductId { get; set; }
		public long CategoryId { get; set; }
		public string? ProductName { get; set; }
		public string? Description { get; set; }
		public int Discount { get; set; }
		public string? Thumbnail { get; set; }
		public virtual ICollection<ProductVariantResponseDTO>? ProductVariants { get; set; }
		public virtual ICollection<ProductMediaResponseDTO>? ProductMedias { get; set; }
		public virtual ICollection<TagResponseDTO>? Tags { get; set; }
	}
	public class ProductVariantResponseDTO
	{
		public long ProductVariantId { get; set; }
		public string? Name { get; set; }
		public long? Price { get; set; }
		public virtual ICollection<AssetInformationResponseDTO>? AssetInformation { get; set; }
	}
	public class AssetInformationResponseDTO
	{
		public long AssetInformationId { get; set; }
		public string? Asset { get; set; }
	}
    public class AllProductResponseDTO
    {
        public long ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Discount { get; set; }
        public List<ProductDetailVariantResponeDTO>? ProductVariants { get; set; }
    }
}
