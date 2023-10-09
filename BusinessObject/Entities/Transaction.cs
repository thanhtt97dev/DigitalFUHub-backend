using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;

namespace BusinessObject.Entities
{
	public class Transaction
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long TransactionId { get; set; }
		public long UserId { get; set; }
		public long TransactionTypeId { get; set; }
		public long OrderId { get; set; }
		public long PaymentAmount { get; set; }
		public string? Note { get; set; } = null!;
		public DateTime? DateCreate { get; set; }

		[ForeignKey(nameof(UserId))]
		public virtual User User { get; set; } = null!;
		[ForeignKey(nameof(OrderId))]
		public virtual Order Order { get; set; } = null!;
		[ForeignKey(nameof(TransactionTypeId))]
		public virtual TransactionType? TransactionType { get; set; } = null!;
	}
}
