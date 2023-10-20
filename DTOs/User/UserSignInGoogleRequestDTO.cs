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
        [Required]
        public string GToken { get; set; } = null!;
    }
}
