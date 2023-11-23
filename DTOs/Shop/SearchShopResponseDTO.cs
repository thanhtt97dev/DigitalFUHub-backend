using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Shop
{
	public class SearchShopResponseDTO
	{
        public long TotalItems { get; set; }
		public List<ShopDetailCustomerResponseDTO> Shops { get; set; } = new();
    }
}
