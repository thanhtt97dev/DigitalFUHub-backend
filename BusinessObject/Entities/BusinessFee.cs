using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class BusinessFee
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long BusinessFeeId { get; set; }
		public long Fee { get; set; }
		public DateTime StartDate { get; set; }
		public virtual ICollection<Order>? Orders { get; set; }
	}
}
