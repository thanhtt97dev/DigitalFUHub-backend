using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    public class Product
    {
        public Product()
        {
            ProductImages = new List<ProductType>();
            Coupons = new List<Coupon>();
            Feedbacks = new List<Feedback>();
            OrderDetails = new List<OrderDetail>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long ProductId { get; set; }
        public long SellerId { get; set; }
        public long CategoryId { get; set; }
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public int Discount { get; set; }
        public long Price { get; set; }
        public long Quantity { get; set; }
        public string? Thumbnail { get; set; }
        public DateTime DateCreate { get; set; }
        public bool Status { get; set; }

        [ForeignKey(nameof(SellerId))]
        public virtual User Seller { get; set; } = null!;
        [ForeignKey(nameof(CategoryId))]
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<ProductType> ProductImages { get; set; }
        public virtual ICollection<Coupon> Coupons { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
