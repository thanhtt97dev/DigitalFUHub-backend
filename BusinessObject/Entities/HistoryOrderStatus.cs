using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class HistoryOrderStatus
	{
		[Key]
		public long OrderId { get; set; }
		[Key]
		public long OrderStatusId { get; set; }
		public DateTime DateCreate { get; set; }
		public string Note { get; set; } = string.Empty;
		[ForeignKey(nameof(OrderId))]
		public virtual Order Order { get; set; } = null!;
		[ForeignKey(nameof(OrderStatusId))]
		public virtual OrderStatus OrderStatus { get; set; } = null!;
	}
}
