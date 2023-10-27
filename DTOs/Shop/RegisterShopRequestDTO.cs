using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Shop
{
	public class RegisterShopRequestDTO
	{
		[Required]
		public long UserId{ get; set; }
		[Required]
        public string ShopName { get; set; } = string.Empty!;
        [Required]
        public string ShopDescription { get; set; } = string.Empty!;
    }
}
