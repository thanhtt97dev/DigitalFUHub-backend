using BusinessObject;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.User
{
    public class UserResponeDTO
    {
        public long UserId { get; set; }
        public long RoleId { get; set; }
        public string? RoleName { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Avatar { get; set; }
        public bool TwoFactorAuthentication { get; set; }
        public bool? Status { get; set; }
        public string? Fullname { get; set; }
    }
}
