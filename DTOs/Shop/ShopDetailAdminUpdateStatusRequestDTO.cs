using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Shop
{
    public class ShopDetailAdminUpdateStatusRequestDTO
    {
        public long ShopId { get; set; }
        public bool IsActive { get; set; } // new status
        public string Note { get; set; } = string.Empty;
    }
}
