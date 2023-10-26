using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Order
{
	public class EditStatusOrderRequestDTO
	{
		public long ShopId { get; set; }
		public long OrderId { get; set;}
		public int StatusId { get; set;}
	}
}
