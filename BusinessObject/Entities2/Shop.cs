using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities2
{
	public class Shop
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long ShopId { get; set; }
		public string? ShopName { get; set; }
		public DateTime DateCreate { get; set; }
		public long Balance { get; set; }
		public string? Description { get; set; }
		public bool IsActive { get; set; }

		public virtual ICollection<Product>? Products { get; set; }
		public virtual ICollection<Coupon>? Coupons { get; set; }
	}
}
