using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class OrderStatus
	{
		[Key]
		public long OrderStatusId { get; set; }
		public string? Name { get; set; }

		public virtual ICollection<Order>? Orders { get; set; }
		public virtual ICollection<HistoryOrderStatus>? HistoryOrderStatus { get; set; }
	}
}
