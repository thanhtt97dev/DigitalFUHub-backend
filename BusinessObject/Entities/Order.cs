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
        public long BusinessFeeId { get; set; }
		public long? FeedbackId { get; set; }
		public long OrderStatusId { get; set; }
		public int Quantity { get; set; }
		public long Price { get; set; }
		public long Discount { get; set; }
		public DateTime OrderDate { get; set; }
		public long TotalDiscount { get; set; }
		public long TotalAmount { get; set; }
		public string? Note { get; set; }

		[ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
        [ForeignKey(nameof(OrderStatusId))]
        public virtual OrderStatus? OrderStatus { get; set; } = null!;
        [ForeignKey(nameof(ProductVariantId))]
        public virtual ProductVariant ProductVariant { get; set; } = null!;

		[ForeignKey(nameof(BusinessFeeId))]
		public virtual BusinessFee BusinessFee { get; set; } = null!;
		[ForeignKey(nameof(FeedbackId))]
		public virtual Feedback? Feedback { get; set; }
		public virtual List<AssetInformation> AssetInformations { get; set; } = null!;
		public virtual List<OrderCoupon> OrderCoupons { get; set; } = null!;
	}
}
