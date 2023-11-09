using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Seller
{
	public class SellerEditShopRequestDTO
	{
		public long UserId { get; set; }
		public string ShopDescription { get; set; } = string.Empty!;
		public IFormFile AvatarFile { get; set; } = null!;
	}
}
