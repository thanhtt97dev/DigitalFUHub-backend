using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    public class Media
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long MediaId { get; set; }

        public long ForeignId { get; set; }
        public long MediaTypeId { get; set; }
        public string? Url { get; set; }

        [ForeignKey(nameof(MediaTypeId))]
        public virtual MediaType MediaType { get; set; } = null!;

	}
}
