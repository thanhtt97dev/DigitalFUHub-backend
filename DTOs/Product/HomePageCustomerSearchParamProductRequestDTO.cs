using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Product
{
    public class HomePageCustomerSearchParamProductRequestDTO
    {
        public long CategoryId { get; set; }
        public bool IsOrderFeedback { get; set; } = false;
        public bool IsOrderSoldCount { get; set; } = false;
        public int Page { get; set; }
    }
}
