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
		public long ShopId { get; set; }
        public long BusinessFeeId { get; set; }
		public long OrderStatusId { get; set; }
		public DateTime OrderDate { get; set; }
		public string? Note { get; set; }
		public long TotalAmount { get; set; }
		public long TotalCouponDiscount { get; set; }
		public long TotalCoinDiscount { get; set; }
		public long TotalPayment { get; set; }

		[ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
		[ForeignKey(nameof(ShopId))]
		public virtual Shop Shop { get; set; } = null!;
		[ForeignKey(nameof(BusinessFeeId))]
		public virtual BusinessFee BusinessFee { get; set; } = null!;
		[ForeignKey(nameof(OrderStatusId))]
        public virtual OrderStatus? OrderStatus { get; set; } = null!;
		public virtual ICollection<OrderCoupon> OrderCoupons { get; set; } = null!;
		public virtual ICollection<OrderDetail> OrderDetails { get; set; } = null!;
		public virtual ICollection<TransactionCoin>? TransactionCoin { get; set; }
		public virtual ICollection<TransactionInternal>? TransactionInternal { get; set; }
	}
}
