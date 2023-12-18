using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class ShopRegisterFee
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long ShopRegisterFeeId { get; set; }
		public long Fee { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public virtual ICollection<Shop>? Shops { get; set; }
	}
}
