using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Seller
{
	public class AddProductRequestDTO
	{
		public long UserId { get; set; }
		public string ProductName { get; set;} = null!;
		public int Discount { get; set;}
		public string? Description { get; set;}
		public int Category { get; set;}
		public IFormFile ThumbnailFile { get; set; } = null!;
		public List<IFormFile> ProductDetailImageFiles { get; set; } = null!;
		public List<string> Tags { get; set; } = null!;
		public List<string> ProductVariantNames { get; set; } = null!;
		public List<long> ProductVariantPrices { get; set; } = null!;
		public List<IFormFile> AssetInformationFiles { get; set; } = null!;
	}
}
