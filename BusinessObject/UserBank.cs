using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
	public class UserBank
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long UserBankId { get; set; }
		public long UserId { get; set; }	
		public long BankId { get; set; }
		public string CreditAccount { get; set; } = null!;
		public string CreditAccountName { get; set; } = null!;

		[ForeignKey(nameof(BankId))]
		public virtual Bank Bank { get; set; } = null!;

		[ForeignKey(nameof(UserId))]
		public virtual User User { get; set; } = null!;
	}
}
