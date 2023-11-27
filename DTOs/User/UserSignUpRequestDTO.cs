using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.User
{
    public class UserSignUpRequestDTO
    {
        [Required]
        [RegularExpression("^(?=[a-z])[a-z\\d]{6,12}$")]
        public string Username { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        [Required]
        [Compare(nameof(Password))]
		public string ConfirmPassword { get; set; } = string.Empty;
        [Required]
        public string Fullname { get; set; } = string.Empty;
    }
}
