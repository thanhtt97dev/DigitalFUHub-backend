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
		public AddProductRequestDTO(long userId, string name, string description, int category, IFormFile thumbnail, List<IFormFile> images, List<AddProductVariantRequestDTO> productVariant, List<string> tags)
		{
			UserId = userId;
			Name = name;
			Description = description;
			Category = category;
			Thumbnail = thumbnail;
			Images = images;
			ProductVariant = productVariant;
			Tags = tags;
		}

		public long UserId { get; set; }
		public string Name { get; set;}
		public string Description { get; set;}
		public int Category { get; set;}
		public IFormFile Thumbnail { get; set;}
		public List<IFormFile> Images { get; set;}
		public List<AddProductVariantRequestDTO> ProductVariants { get; set;}
		public List<string> Tags { get; set;}
	}

	public class AddProductVariantRequestDTO
	{
		public AddProductVariantRequestDTO(string name, long price, List<string> data)
		{
			Name = name;
			Price = price;
			Data = data;
		}

		public string Name { get; set; }
		public long Price { get; set;}
		public List<string> Data { get; set;}
	}
}
