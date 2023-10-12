using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class TransactionCoinType
	{
		[Key]
		public int TransactionCoinTypeId { get; set; }
		public string Name { get; set; } = string.Empty;
		public virtual ICollection<TransactionCoin>? TransactionCoin { get; set; }
	}
}
