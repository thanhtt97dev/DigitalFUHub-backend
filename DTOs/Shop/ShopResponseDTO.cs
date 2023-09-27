using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Shop
{
    public class ShopResponseDTO
    {
        public long UserId { get; set; }
        public string? ShopName { get; set; }
        public DateTime DateCreate { get; set; }
        public bool IsActive { get; set; }
    }
}
