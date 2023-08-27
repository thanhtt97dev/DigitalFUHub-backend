using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
	public class Storage
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
        public long ImageId { get; set; }

		public long UserId { get; set; }
		public string? FileName { get; set; }
		public bool IsPublic { get; set; }

		[ForeignKey(nameof(UserId))]
		public virtual User User{ get; set; } = null!;

	}
}
