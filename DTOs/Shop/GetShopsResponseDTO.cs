using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Shop
{
	public class GetShopsResponseDTO
	{
		public long ShopId { get; set; }
		public string ShopName { get; set; } = null!;
		public string ShopEmail { get; set; } = null!;
		public string Avatar { get; set; } = string.Empty;
		public DateTime DateCreate { get; set; }
		public int TotalProduct { get; set; }
		public int NumberOrderConfirmed { get; set; }
		public int TotalNumberOrder { get; set; }
		public long Revenue { get;set; }
		public bool IsActive { get; set; }
	}
}
