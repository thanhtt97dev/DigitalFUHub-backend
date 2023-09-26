using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.User
{
    public class UserUpdateRequestDTO
    {
        public long? RoleId { get; set; }
        public string? Fullname { get; set; }
        public string? Avatar { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }

    }
}
