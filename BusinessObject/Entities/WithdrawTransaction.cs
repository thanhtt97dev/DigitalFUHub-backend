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
		public long UserBankId { get; set; }
		public long UserId { get; set; }
		public long WithdrawTransactionStatusId { get; set; }	
		public DateTime RequestDate { get; set; }
		public DateTime? PaidDate { get; set; }
		public string Code { get; set; } = null!;
		public long Amount { get; set; }
		public string? Note { get; set; }

		[ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
		[ForeignKey(nameof(UserBankId))]
		public virtual UserBank UserBank { get; set; } = null!;
		[ForeignKey(nameof(WithdrawTransactionStatusId))]
		public virtual WithdrawTransactionStatus WithdrawTransactionStatus { get; set; } = null!;
	}
}
