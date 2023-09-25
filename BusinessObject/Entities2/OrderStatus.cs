using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities2
{
	public class OrderStatus
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long OrderStatusId { get; set; }
		public string? Name { get; set; }

		public virtual ICollection<Order>? Orders { get; set; } 
	}
}
