using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Product
{
	public class ProductDetailAdminResponseDTO
	{
		public long ProductId { get; set; }
		public string ProductName { get; set; } = string.Empty;
		public long CategoryId { get; set; }
		public string? CategoryName { get; set; }
		public long ShopId { get; set; }
		public string ShopName { get; set; } = null!;
		public string ShopAvatar { get; set; } = string.Empty;
		public string? Description { get; set; }
		public int Discount { get; set; }
		public string? Thumbnail { get; set; }
		public DateTime DateCreate { get; set; }
		public DateTime DateUpdate { get; set; }
		public long TotalRatingStar { get; set; }
		public long NumberFeedback { get; set; }
		public int ViewCount { get; set; }
		public int LikedCount { get; set; }
		public int SoldCount { get; set; }
		public string Note { get; set; } = string.Empty;
		public long ProductStatusId { get; set; }
		public List<ProductDetailProductVariantAdminResponseDTO> ProductVariants { get; set; } = new List<ProductDetailProductVariantAdminResponseDTO>();
		public List<string>? Tags { get; set; } = new List<string>();
		public List<string>? ProductMedias { get; set; } = new List<string>();
		public List<ProductDetailReportProductAdminResponseDTO>? ReportProducts { get; set; } = new List<ProductDetailReportProductAdminResponseDTO>();
	}

	public class ProductDetailProductVariantAdminResponseDTO
	{
		public string? Name { get; set; }
		public long Price { get; set; }
	}


	public class ProductDetailProductMediaAdminResponseDTO
	{
		public string Url { get; set; } = string.Empty;
	}

	public class ProductDetailReportProductAdminResponseDTO
	{
		public long ReportProductId { get; set; }
		public long UserId { get; set; }
		public string Email { get; set; } = string.Empty;
		public string Avatar { get; set; } = string.Empty;
		public int ReasonReportProductId { get; set; }
		public string ViName { get; set; } = string.Empty;
		public string ViExplanation { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public DateTime DateCreate { get; set; }
		public string Note { get; set; } = string.Empty;
		public int ReportProductStatusId { get; set; }
	}



}
