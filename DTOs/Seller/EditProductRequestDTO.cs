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
		public long UserId { get; set; }
		public long ProductId { get; set; }
		public string ProductName { get; set; } = null!;
		public string ProductDescription{ get; set; } = null!;
		//public int Discount{ get; set; }
		public int CategoryId{ get; set; }
		public IFormFile ProductThumbnailFileUpdate{ get; set; } = null!; 
		public List<string> ProductDetailImagesCurrent { get; set; } = new();
		public List<IFormFile> ProductDetailImagesAddNew { get; set; } = new(); 
		public List<long> ProductVariantIdsUpdate { get; set; } = new(); 
		public List<string> ProductVariantNamesUpdate { get; set; } = new();
		public List<long> ProductVariantPricesUpdate { get; set; } = new(); 
		public List<int> ProductVariantDiscountsUpdate { get; set; } = new(); 
		public List<IFormFile> AssetInformationFilesUpdate { get; set; } = new(); 
		public List<string> ProductVariantNamesAddNew { get; set; } = new(); 
		public List<long> ProductVariantPricesAddNew { get; set; } = new();
		public List<int> ProductVariantDiscountsAddNew { get; set; } = new();
		public List<IFormFile> AssetInformationFilesAddNew { get; set; } = new(); 
		public List<string> Tags { get; set; } = new(); 
		public bool IsActiveProduct { get; set; }
	}
}
