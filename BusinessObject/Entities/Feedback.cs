using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    public class Feedback
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long FeedbackId { get; set; }
        public long ProductId { get; set; }
        public long UserId { get; set; }
        public string? Content { get; set; }
        public int Rate { get; set; }
        public DateTime UpdateAt { get; set; }
        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; } = null!;
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
        public virtual Order? Order { get; set; }
		public virtual ICollection<FeedbackMedia> FeedbackMedias { get; set; } = null!;
    }
}
