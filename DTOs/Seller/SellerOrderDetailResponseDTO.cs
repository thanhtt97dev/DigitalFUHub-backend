using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Seller
{
	public class SellerOrderDetailResponseDTO
	{
        public long OrderId { get; set; }
		public string EmailCustomer { get; set; } = string.Empty;
		public long Price{ get; set; }
		public int Quantity{ get; set; }
		public DateTime OrderDate{ get; set; }
		public bool IsFeedback { get; set; }
		public long OrderStatusId { get; set; }
		public List<SellerProductResponseDTO> Products { get; set; } = null!;
    }
	
}
