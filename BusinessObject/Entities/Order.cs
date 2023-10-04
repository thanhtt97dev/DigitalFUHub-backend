using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    public class Order
    {
       

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long OrderId { get; set; }
        public long UserId { get; set; }
        public long ProductVariantId { get; set; }
        public long PlatformFeeId { get; set; }
		public long AssetInformationId { get; set; }
		public long Quantity { get; set; }
		public long Price { get; set; }
		public DateTime OrderDate { get; set; }
        public long TotalAmount { get; set; }
        public bool IsFeedback { get; set; }
        public long OrderStatusId { get; set; }
        public DateTime DateOrder { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
        [ForeignKey(nameof(OrderStatusId))]
        public virtual OrderStatus? OrderStatus { get; set; } = null!;
        [ForeignKey(nameof(ProductVariantId))]
        public virtual ProductVariant? ProductVariant { get; set; } = null!;

		[ForeignKey(nameof(PlatformFeeId))]
		public virtual PlatformFee? PlatformFee { get; set; } = null!;
		[ForeignKey(nameof(AssetInformationId))]
		public virtual AssetInformation AssetInformation { get; set; } = null!;
		public virtual List<OrderCoupon>? OrderCoupons { get; set; }
    }
}
