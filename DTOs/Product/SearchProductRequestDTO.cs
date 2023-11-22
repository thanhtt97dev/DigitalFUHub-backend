using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Product
{
	public class SearchProductRequestDTO
	{
        public string Keyword { get; set; } = string.Empty;
        public long Category { get; set; }
        public int Rating { get; set; }
        public long Sort { get; set; }
        public long? MinPrice { get; set; } = null!;
        public long? MaxPrice { get; set; } = null!;
        public int Page { get; set; } = 1;
    }
	public class SearchProductResponseDTO
	{
		public long TotalItems { get; set; }
        public List<HomePageCustomerProductDetailResponseDTO> Products { get; set; } = new();
	}
}
