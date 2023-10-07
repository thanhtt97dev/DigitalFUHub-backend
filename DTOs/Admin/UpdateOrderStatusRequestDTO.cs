using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Admin
{
	public class UpdateOrderStatusRequestDTO
	{
		public long OrderId { get; set; }	
		public int Status { get; set; }
		public string? Note { get; set; }	
	}
}
