using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.User
{
    public class SettingPersonalUpdateAvatarUserRequestDTO
    {
        public long UserId { get; set; }
        public IFormFile? Avatar { get; set; }
    }
}
