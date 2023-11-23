using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.WishList
{
    public class WishListCustomerAddRequestDTO
    {
        public long UserId { get; set; }
        public long ProductId { get; set; }
    }
}
