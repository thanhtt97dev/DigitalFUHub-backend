using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Product
{
	public class GetProductsResponseDTO
	{
		public int TotalPage { get; set;}
		public int TotalProduct { get; set; }
		public List<GetProductsProductDetailResponseDTO> Products { get; set; } = new List<GetProductsProductDetailResponseDTO>();
	}
	public class GetProductsProductDetailResponseDTO 
	{
		public long ShopId { get; set; }	
		public string ShopName { get; set; } = string.Empty;
		public long ProductId { get; set; }
		public string ProductName { get; set; } = string.Empty;
		public string ProductThumbnail { get; set; } = string.Empty;
		public long ProductStatusId { get; set; }
		public int ViewCount { get; set; }
		public int LikedCount { get; set; }
		public int SoldCount { get; set; }
		public List<GetProductsProductVariantDetailResponseDTO> ProductVariants { get; set; } = new List<GetProductsProductVariantDetailResponseDTO>();
	}
	public class GetProductsProductVariantDetailResponseDTO
	{
		public long ProductVariantId { get; set; }
		public string ProductVariantName { get; set; } = string.Empty;
		public long ProductVariantPrice { get; set; }
	}
}
