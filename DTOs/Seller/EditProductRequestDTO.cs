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
		public int Discount{ get; set; }
		public int CategoryId{ get; set; }
		public IFormFile ProductThumbnailFileUpdate{ get; set; } = null!; // ProductThumbnailAddNew
		public List<string> ProductDetailImagesCurrent { get; set; } = new(); // ProductDetailImagesCurrent
		public List<IFormFile> ProductDetailImagesAddNew { get; set; } = new(); // ProductDetailImagesAddNew
		public List<long> ProductVariantIdsUpdate { get; set; } = new(); // ProductVariantIdsUpdate
		public List<string> ProductVariantNamesUpdate { get; set; } = new(); // ProductVariantNamesUpdate
		public List<long> ProductVariantPricesUpdate { get; set; } = new(); // ProductVariantPricesUpdate
		public List<IFormFile> AssetInformationFilesUpdate { get; set; } = new(); // AssetInformationFilesUpdate
		public List<string> ProductVariantNamesAddNew { get; set; } = new(); // ProductVariantNamesAddNew
		public List<long> ProductVariantPricesAddNew { get; set; } = new(); // ProductVariantPricesAddNew
		public List<IFormFile> AssetInformationFilesAddNew { get; set; } = new(); //AssetInformationFilesAddNew
		public List<string> Tags { get; set; } = new(); // Tags
		public bool IsActiveProduct { get; set; }
	}
}
