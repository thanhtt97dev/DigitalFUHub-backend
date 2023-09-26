using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class TransactionType
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long TransactionTypeId { get; set; }
		public string Name { get; set; } = null!;

		public virtual ICollection<Transaction>? Transactions { get; set;}
	}
}
