using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    public class ProductVariant
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long ProductVariantId { get; set; }
        public long ProductId { get; set; }
        public string? Name { get; set; }
        public long? Price { get; set; }
		public bool isActivate { get; set; }

		[ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; } = null!;
        public virtual Order Order { get; set; } = null!;
        public virtual ICollection<Cart> Carts { get; set; } = null!;
		public virtual ICollection<AssetInformation> AssetInformation { get; set; } = null!;
	}
}
