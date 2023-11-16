using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Product
{
    public class ShopDetailCustomerSearchParamProductRequestDTO
    {
        public long UserId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Page { get; set; }
    }
}
