using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Product
{
    public class ProductMediaResponseDTO
    {
        public long ProductMediaId { get; set; }
        public string Url { get; set; } = null!;
    }
}
