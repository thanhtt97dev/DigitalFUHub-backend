using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.User
{
    public class SettingPersonalUpdateFullNameUserRequestDTO
    {
        public long UserId { get; set; }
        public string Fullname { get; set; } = string.Empty!;
    }
}
