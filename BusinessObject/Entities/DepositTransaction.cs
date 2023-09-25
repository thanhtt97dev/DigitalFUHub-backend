using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    public class DepositTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long DepositTransactionId { get; set; }
        public long UserId { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public string Code { get; set; } = null!;
        public long Amount { get; set; }
        public bool IsPay { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; } = null!;
    }
}
