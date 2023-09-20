using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
    public class Image
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ImageId { get; set; }
        public long MessageId { get; set; }

        [StringLength(255)] // Độ dài tối đa cho đường dẫn hoặc tên tệp ảnh
        public string ImagePath { get; set; } = string.Empty!;
        public DateTime Timestamp { get; set; }

        [ForeignKey(nameof(MessageId))]
        public virtual Message Message { get; set; } = null!;
    }
}
