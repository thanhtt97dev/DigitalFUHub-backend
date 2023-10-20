using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.User
{
    public class UserOnlineStatusResponseDTO
    {
        public long UserId { get; set; }
        public DateTime LastTimeOnline { get; set; }
    }
}
