﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.User
{
    public class ChangePasswordRequestDTO
    {
        public long UserId { get; set; }
        public string OldPassword { get; set; } = string.Empty!;
        public string NewPassword { get; set; } = string.Empty!;
    }
}
