using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class WithdrawTransaction
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long WithdrawTransactionId { get; set; }
		public long WithdrawTransactionStatusId { get; set; }
		public long UserId { get; set; }
		public DateTime RequestDate { get; set; }
		public DateTime PaidDate { get; set; }
		public long Amount { get; set; }
		public bool IsPay { get; set; }

		[ForeignKey(nameof(WithdrawTransactionStatusId))]
		public virtual WithdrawTransactionStatus? WithdrawTransactionStatus { get; set; }
	}
}
