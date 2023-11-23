using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.WishList
{
    public class WishListCustomerParamRequestDTO
    {
        public long UserId { get; set; }
        public int Page { get; set; }
    }
}
