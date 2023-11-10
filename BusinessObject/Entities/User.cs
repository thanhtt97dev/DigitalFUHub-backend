using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml.Style;

namespace BusinessObject.Entities
{
    public class User
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UserId { get; set; }
        public long RoleId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Fullname { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public bool Status { get; set; }
        public bool TwoFactorAuthentication { get; set; }
        public long AccountBalance { get; set; }
		public long Coin { get; set; }
		public bool SignInGoogle { get; set; }
        public bool IsConfirm { get; set; }
		public DateTime LastTimeOnline { get; set; }
		public bool IsOnline { get; set; }

		[ForeignKey(nameof(RoleId))]
        public virtual Role Role { get; set; } = null!;
        public virtual Shop Shop { get; set; } = null!;
        public virtual ICollection<AccessToken>? AccessTokens { get; set; }
        public virtual ICollection<DepositTransaction>? DepositTransactions { get; set; }
        public virtual ICollection<UserConversation>? UserConversations { get; set; }
        public virtual ICollection<UserBank>? UserBanks { get; set; }
        public virtual ICollection<Order>? Orders { get; set; }
        public virtual ICollection<Feedback>? Feedbacks { get; set; }
        public virtual ICollection<TransactionInternal>? TransactionInternals { get; set; }
        public virtual ICollection<WithdrawTransaction>? WithdrawTransactions { get; set; }
        public virtual ICollection<Cart>? Carts { get; set; }
        public virtual ICollection<TransactionCoin>? TransactionCoins { get; set; }
		public virtual ICollection<WishList>? WishList { get; set; }
		public virtual ICollection<ReportProduct>? ReportProducts { get; set; }

	}
}
