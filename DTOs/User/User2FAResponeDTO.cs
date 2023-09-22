using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.User
{
    public class User2FAResponeDTO
    {
        public string SecretKey { get; set; } = null!;
        public string QRCode { get; set; } = null!;
    }
}
