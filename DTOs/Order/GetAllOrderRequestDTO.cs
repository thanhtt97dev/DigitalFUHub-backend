using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Order
{
	public class GetAllOrderRequestDTO
	{
		public List<long> StatusId { get; set; } = null!;
		public int Limit { get; set; }
		public int Offset { get; set; }
	}
}
