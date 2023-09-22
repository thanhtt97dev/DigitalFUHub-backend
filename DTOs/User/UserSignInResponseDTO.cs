using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.User
{
    public class UserSignInResponseDTO
    {
        public string? JwtId { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public long UserId { get; set; }
        public string? Username { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Fullname { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string? RoleName { get; set; }
        public bool TwoFactorAuthentication { get; set; }
        public bool SignInGoogle { get; set; }
    }
}
