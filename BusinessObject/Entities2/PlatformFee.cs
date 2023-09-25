using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities2
{
	public class PlatformFee
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long PlatformFeeId { get; set; }
		public long Fee { get; set; }
		public DateTime StartDate { get; set; }
		public virtual ICollection<Order>? Orders { get; set; }
	}
}
