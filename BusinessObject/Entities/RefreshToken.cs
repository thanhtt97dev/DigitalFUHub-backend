using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    public class RefreshToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long UserId { get; set; }
        public string? TokenRefresh { get; set; }
        public DateTime ExpiredDate { get; set; }
        public string? JwtId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; } = null!;
    }
}
