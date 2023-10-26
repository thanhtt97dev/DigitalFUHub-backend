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
		public string? ProductName { get; set;}
		public int Discount { get; set;}
		public string? Description { get; set;}
		public int Category { get; set;}
		public IFormFile Thumbnail { get; set; } = null!;
		public List<IFormFile> Images { get; set; } = null!;
		public List<string> Tags { get; set; } = null!;
		public List<string> NameVariants { get; set; } = null!;
		public List<long> PriceVariants{ get; set; } = null!;
		public List<IFormFile> DataVariants { get; set; } = null!;
	}
}
