using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities2
{
	public class MediaType
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		public long MediaTypeId { get; set; }
		public string? Name { get; set; }
	}
}
