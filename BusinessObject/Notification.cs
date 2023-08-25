using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
	public class Notification
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long NotificationId { get; set; }
		public long UserId { get; set; }
		public string? Title { get; set; }
		public string? Content { get; set; }
		public string? Link { get; set; }
		public DateTime DateCreated { get; set; }
		public bool IsReaded { get; set; }

		[ForeignKey(nameof(UserId))]
		public virtual User User { get; set; } = null!;
	}
}
