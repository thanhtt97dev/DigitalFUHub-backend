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
        public long TotalRatingStar { get; set; }
        public long NumberFeedback { get; set; }
        public bool IsActive { get; set; }
        public int TotalNumberOrder { get; set; }
        public int TotalNumberProduct { get; set; }
        public int NumberProductsSold { get; set; }
        public long Revenue { get; set; }
        public int NumberOrderWaitConfirmation { get; set; }
        public int NumberOrderConfirmed { get; set; }
        public int NumberOrderViolated { get; set; }
        public ShopDetailAdminUserResponseDTO? User { get; set; }
    }

    public class ShopDetailAdminUserResponseDTO
    {
        public bool IsOnline { get; set; }
        public DateTime LastTimeOnline { get; set; }
    }
}
