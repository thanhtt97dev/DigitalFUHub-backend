using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Admin
{
	public class UserInfoResponseDTO
	{
		public long UserId { get; set; }
		public string Role { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Username { get; set; } = string.Empty;
		public string Fullname { get; set; } = string.Empty;
		public string Avatar { get; set; } = string.Empty;
		public long AccountBalance { get; set; }
		public long Coin { get; set; }
		public bool Status { get; set; }
		public bool IsConfirm { get; set; }
		public bool TwoFactorAuthentication { get; set; }
		public DateTime LastTimeOnline { get; set; }
		public bool IsOnline { get; set; }
		public DateTime? BanDate { get; set; } = null!;
		public string? Note { get; set; } = null!;
		public long NumberOrdersBuyed { get; set; }
		public long TotalAmountOrdersBuyed { get; set; }
		public long NumberOrderSold { get; set; }
		public long Profit { get; set; }
	}
}
