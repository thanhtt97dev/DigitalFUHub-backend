using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Cart
{
	public class DeleteCartRequestDTO
	{
		public long CartId { get; set; }	
		public List<long> CartDetailIds { get; set; }  = new List<long>();	
	}
}
