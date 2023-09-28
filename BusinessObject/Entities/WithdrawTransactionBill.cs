using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class WithdrawTransactionBill
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long WidrawTransactionBillId { get; set; }
		public long WithdrawTransactionId { get; set; }

		[ForeignKey(nameof(WithdrawTransactionId))]
		public virtual WithdrawTransaction WithdrawTransaction { get; set; } = null!;
		public DateTime? PostingDate { get; set; }
		public DateTime? TransactionDate { get; set; }
		public string? AccountNo { get; set; }
		public int CreditAmount { get; set; }
		public int DebitAmount { get; set; }
		public string? Currency { get; set; }
		public string Description { get; set; } = null!;
		public int AvailableBalance { get; set; }
		public string? BeneficiaryAccount { get; set; }
		public string RefNo { get; set; } = null!;
		public string? BenAccountName { get; set; }
		public string? BankName { get; set; }
		public string BenAccountNo { get; set; } = string.Empty;
		public DateTime? DueDate { get; set; }
		public string? DocId { get; set; }
		public string? TransactionType { get; set; }
	}
}
