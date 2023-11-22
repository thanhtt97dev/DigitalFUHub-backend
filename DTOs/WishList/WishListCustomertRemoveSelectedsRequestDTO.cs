using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.WishList
{
    public class WishListCustomertRemoveSelectedsRequestDTO
    {
        public long UserId { get; set; }
        public List<long> ProductIds { get; set; } = new List<long>();
    }
}
