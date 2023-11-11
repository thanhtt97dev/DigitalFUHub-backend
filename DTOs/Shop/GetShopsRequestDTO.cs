using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Shop
{
	public class GetShopsRequestDTO
	{
		public long ShopId { get; set; }
		public string ShopEmail { get; set; } = string.Empty;
		public string ShopName { get; set; } = string.Empty;
		public int ShopStatusId { get; set; }	
		public int Page { get; set; }
	}
}
