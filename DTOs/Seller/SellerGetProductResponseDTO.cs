using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Seller
{
	public class SellerGetProductResponseDTO
	{
        public long TotalItems { get; set; }
		public List<BusinessObject.Entities.Product> Products { get; set; } = new();
    }
}
