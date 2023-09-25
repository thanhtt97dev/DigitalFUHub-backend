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
	public class AssetInformation
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long AssetInformationId { get; set; }
		public long ProductTypeId { get; set; }
		public long? UserId { get; set; }
		public DateTime? CreateDate { get; set; }
		public DateTime? UpdateDate { get; set; }
		public string? Asset { get; set; }
		public bool IsActive { get; set; }

		[ForeignKey(nameof(UserId))]
		public virtual User? User { get; set; }
		[ForeignKey(nameof(ProductTypeId))]
		public virtual ProductType? ProductType { get; set; }
	}
}
