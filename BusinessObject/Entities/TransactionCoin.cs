using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class TransactionCoin
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]	
		public long TransactionCoinId { get; set; }	
		public long UserId { get; set; }	
		public long? OrderId { get; set; }
		public long? FeedbackId { get; set; }
		public int TransactionCoinTypeId { get; set; }	
		public long Amount { get; set; }	
		public string? Note { get; set; }	
		public DateTime DateCreate { get; set; }

		[ForeignKey(nameof(UserId))]
		public virtual User User { get; set; } = null!;
		[ForeignKey(nameof(OrderId))]
		public virtual Order? Order { get; set; }
		[ForeignKey(nameof(FeedbackId))]
		public virtual Feedback? Feedback { get; set; }
		[ForeignKey(nameof(TransactionCoinTypeId))]
		public virtual TransactionCoinType? TransactionCoinType { get; set; }

	}
}
