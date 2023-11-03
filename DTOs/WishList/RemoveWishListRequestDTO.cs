using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.WishList
{
    public class RemoveWishListRequestDTO
    {
        public long UserId { get; set; }
        public long ProductId { get; set; }
    }
}
