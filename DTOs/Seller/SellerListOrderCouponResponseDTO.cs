using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Seller
{
	public class SellerListOrderCouponResponseDTO
	{
		public long TotalItems { get; set; }
		public List<SellerOrderResponseDTO> Orders { get; set; } = new();
	}
}
