using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class WithdrawTransactionStatus
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long WithdrawTransactionStatusId { get; set; }
		public string? Name { get; set; }

		public virtual ICollection<WithdrawTransaction>? WithdrawTransaction { get; set; }
	}
}
