﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.User
{
	public class UserConfirmEmailRequestDTO
	{
		public string Token { get; set; } = null!;
    }
}
