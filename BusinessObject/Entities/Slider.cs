using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    public class Slider
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SliderId { get; set; }
        public string Name { get; set; } = string.Empty!;
        public string Url { get; set; } = string.Empty!;
        public string Link { get; set; } = string.Empty;
        public DateTime DateCreate { get; set; }
        public bool IsActive { get; set; }
    }
}
