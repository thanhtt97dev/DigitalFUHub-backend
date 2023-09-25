using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.User
{
    public class UserSignInGoogleRequestDTO
    {
		public UserSignInGoogleRequestDTO(string email, string fullname)
		{
			Email = email;
			Fullname = fullname;
		}

		[Required]
        public string Email { get; set; }
		[Required]
		public string Fullname { get; set; }
    }
}
