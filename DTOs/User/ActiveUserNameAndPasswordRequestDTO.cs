using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.User
{
    public class ActiveUserNameAndPasswordRequestDTO
    {
        public long UserId { get; set; }
        public string Username { get; set; } = string.Empty!;
        public string Password { get; set; } = string.Empty!;
    }
}
