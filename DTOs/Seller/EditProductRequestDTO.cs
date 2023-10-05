using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Seller
{
	public class EditProductRequestDTO
	{
		public long ProductId { get; set; }
		public long UserId { get; set; }
		public string ProductName { get; set; } = null!;
		public string ProductDescription{ get; set; } = null!;
		public int Discount{ get; set; }
		public int CategoryId{ get; set; }
		public IFormFile ProductThumbnail { get; set; } = null!;
		public List<string> ProductImagesOld { get; set; } = new();
		public List<IFormFile> ProductImagesNew { get; set; } = new();
		public List<long> ProductVariantIdUpdate { get; set; } = new();
		public List<string> ProductVariantNameUpdate { get; set; } = new();
		public List<long> ProductVariantPriceUpdate { get; set; } = new();
		public List<IFormFile> ProductVariantFileUpdate { get; set; } = new();
		public List<string> ProductVariantNameNew { get; set; } = new();
		public List<long> ProductVariantPriceNew { get; set; } = new();
		public List<IFormFile> ProductVariantFileNew { get; set; } = new();
		public List<string> Tags { get; set; } = new();
	}
}
