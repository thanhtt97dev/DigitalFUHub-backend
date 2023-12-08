using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Shop
{
    public class ShopDetailAdminResponseDTO
    {
        public long UserId { get; set; }
        public string Avatar { get; set; } = string.Empty;
        public string ShopEmail { get; set; } = null!;
        public string ShopName { get; set; } = null!;
        public DateTime DateCreate { get; set; }
        public DateTime DateBan { get; set; }
        public string Note { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long TotalRatingStar { get; set; }
        public long NumberFeedback { get; set; }
        public bool IsActive { get; set; }
        public ShopDetailAdminUserResponseDTO? User { get; set; }
    }

    public class ShopDetailAdminUserResponseDTO
    {
        public bool IsOnline { get; set; }
        public DateTime LastTimeOnline { get; set; }
    }
}
