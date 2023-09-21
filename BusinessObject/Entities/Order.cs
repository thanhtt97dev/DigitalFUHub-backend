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
        public Order()
        {
            OrderDetails = new List<OrderDetail>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long OrderId { get; set; }
        public long BuyerId { get; set; }
        public long TotalCost { get; set; }
        public bool IsFeedback { get; set; }
        public bool Status { get; set; }
        public DateTime DateOrder { get; set; }

        [ForeignKey(nameof(BuyerId))]
        public virtual User Buyer { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
