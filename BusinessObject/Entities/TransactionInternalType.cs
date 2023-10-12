using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class TransactionInternalType
	{
		[Key]
		public long TransactionInternalTypeId { get; set; }
		public string Name { get; set; } = null!;

		public virtual ICollection<TransactionInternal>? Transactions { get; set;}
	}
}
