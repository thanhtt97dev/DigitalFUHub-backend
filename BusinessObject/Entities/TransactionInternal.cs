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
	public class TransactionInternal
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long TransactionInternalId { get; set; }
		public long UserId { get; set; }
		public long TransactionInternalTypeId { get; set; }
		public long? OrderId { get; set; }
		public long PaymentAmount { get; set; }
		public string? Note { get; set; } = null!;
		public DateTime? DateCreate { get; set; }

		[ForeignKey(nameof(UserId))]
		public virtual User User { get; set; } = null!;
		[ForeignKey(nameof(OrderId))]
		public virtual Order? Order { get; set; } = null!;
		[ForeignKey(nameof(TransactionInternalTypeId))]
		public virtual TransactionInternalType? TransactionType { get; set; } = null!;
	}
}
