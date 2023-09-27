using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Tag
{
    public class TagResponseDTO
    {
        public long TagId { get; set; }
        public string? TagName { get; set; }
    }
}
