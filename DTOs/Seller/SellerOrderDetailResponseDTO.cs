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
		public bool IsFeedbacked { get; set; }
		public long OrderStatusId { get; set; }
		public string ProductName { get; set; } = string.Empty; 
		public string ProductVariantName { get; set;} = string.Empty;
		public string Thumbnail { get; set; } = string.Empty;
    }
	
}
