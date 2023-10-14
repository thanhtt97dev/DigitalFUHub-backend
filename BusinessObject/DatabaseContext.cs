using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System;
using System.Reflection.Metadata;

namespace BusinessObject
{
	public class DatabaseContext : DbContext
	{

		public readonly string connectionString = "server=localhost; database=DigitalFuHub; uid=sa; pwd=sa;MultipleActiveResultSets=true";
		//public readonly string connectionString = "Server=tcp:fptu-database.database.windows.net,1433;Database=fptu;User ID=fptu;Password=A0336687454a;Trusted_Connection=False;Encrypt=True;";

		public DatabaseContext() { }

		public DatabaseContext(DbContextOptions options) : base(options) { }

		#region DbSet
		public virtual DbSet<Role> Role { get; set; } = null!;
		public virtual DbSet<User> User { get; set; } = null!;
		public virtual DbSet<AccessToken> AccessToken { get; set; } = null!;
		public virtual DbSet<RefreshToken> RefreshToken { get; set; } = null!;
		public virtual DbSet<Notification> Notification { get; set; } = null!;
		public virtual DbSet<TwoFactorAuthentication> TwoFactorAuthentication { get; set; } = null!;
		public virtual DbSet<DepositTransaction> DepositTransaction { get; set; } = null!;
		public virtual DbSet<WithdrawTransaction> WithdrawTransaction { get; set; } = null!;
		public virtual DbSet<WithdrawTransactionStatus> WithdrawTransactionStatus { get; set; } = null!;
		public virtual DbSet<WithdrawTransactionBill> WithdrawTransactionBill { get; set; } = null!;
		public virtual DbSet<TransactionInternal> TransactionInternal { get; set; } = null!;
		public virtual DbSet<TransactionInternalType> TransactionInternalType { get; set; } = null!;
		public virtual DbSet<UserConversation> UserConversation { get; set; } = null!;
		public virtual DbSet<Conversation> Conversations { get; set; } = null!;
		public virtual DbSet<Message> Messages { get; set; } = null!;
		public virtual DbSet<Bank> Bank { get; set; } = null!;
		public virtual DbSet<UserBank> UserBank { get; set; } = null!;
		public virtual DbSet<Product> Product { get; set; } = null!;
		public virtual DbSet<ProductVariant> ProductVariant { get; set; } = null!;
		public virtual DbSet<ProductStatus> ProductStatus { get; set; } = null!;
		public virtual DbSet<Tag> Tag { get; set; } = null!;
		public virtual DbSet<Shop> Shop { get; set; } = null!;
		public virtual DbSet<Feedback> Feedback { get; set; } = null!;
		public virtual DbSet<FeedbackBenefit> FeedbackBenefit { get; set; } = null!;
		public virtual DbSet<FeedbackMedia> FeedbackMedia { get; set; } = null!;
		public virtual DbSet<ProductMedia> ProductMedia { get; set; } = null!;
		public virtual DbSet<Coupon> Coupon { get; set; } = null!;
		public virtual DbSet<Order> Order { get; set; } = null!;
		public virtual DbSet<OrderStatus> OrderStatus { get; set; } = null!;
		public virtual DbSet<OrderCoupon> OrderCoupon { get; set; } = null!;
		public virtual DbSet<Category> Category { get; set; } = null!;
		public virtual DbSet<Cart> Cart { get; set; } = null!;
		public virtual DbSet<AssetInformation> AssetInformation { get; set; } = null!;
		public virtual DbSet<BusinessFee> BusinessFee { get; set; } = null!;
		public virtual DbSet<TransactionCoin> TransactionCoin { get; set; } = null!;
		public virtual DbSet<TransactionCoinType> TransactionCoinType { get; set; } = null!;

		#endregion



		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(connectionString);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Cart>().HasKey(x => new { x.UserId, x.ProductVariantId });
			modelBuilder.Entity<OrderCoupon>().HasKey(x => new { x.OrderId, x.CouponId });
			modelBuilder.Entity<OrderCoupon>().
				HasOne(x => x.Order)
				.WithMany(x => x.OrderCoupons)
				.HasForeignKey(x => x.OrderId)
				.OnDelete(DeleteBehavior.NoAction);
			modelBuilder.Entity<TransactionInternal>().
				HasOne(x => x.User)
				.WithMany(x => x.TransactionInternals)
				.HasForeignKey(x => x.UserId)
				.OnDelete(DeleteBehavior.NoAction);
			modelBuilder.Entity<Feedback>().
				HasOne(x => x.User)
				.WithMany(x => x.Feedbacks)
				.HasForeignKey(x => x.UserId)
				.OnDelete(DeleteBehavior.NoAction);
			modelBuilder.Entity<Cart>()
				 .HasOne(x => x.User)
				.WithMany(x => x.Carts)
				.HasForeignKey(x => x.UserId)
				.OnDelete(DeleteBehavior.NoAction);
			modelBuilder.Entity<Order>()
				.HasOne(x => x.User)
				.WithMany(x => x.Orders)
				.HasForeignKey(x => x.UserId)
				.OnDelete(DeleteBehavior.NoAction);
			modelBuilder.Entity<Order>()
				.HasOne(x => x.ProductVariant)
				.WithMany(x => x.Orders)
				.HasForeignKey(x => x.ProductVariantId)
				.OnDelete(DeleteBehavior.NoAction);
			modelBuilder.Entity<WithdrawTransaction>()
				.HasOne(x => x.UserBank)
				.WithMany(x => x.WithdrawTransactions)
				.HasForeignKey(x => x.UserBankId)
				.OnDelete(DeleteBehavior.NoAction);

			modelBuilder.Entity<Role>().HasData(new Role[]
			{
				new Role{RoleId = 1, RoleName = "Admin"},
				new Role{RoleId = 2, RoleName = "Customer"},
				new Role{RoleId = 3, RoleName = "Seller"},
			});

			modelBuilder.Entity<Category>().HasData(new Category[]
			{
				new Category{CategoryId = 1, CategoryName = "Mạng xã hội"},
				new Category{CategoryId = 2, CategoryName = "Giáo dục"},
				new Category{CategoryId = 3, CategoryName = "Trò chơi"},
				new Category{CategoryId = 4, CategoryName = "VPS"},
				new Category{CategoryId = 5, CategoryName = "Khác"},
			});

			modelBuilder.Entity<TransactionInternalType>().HasData(new TransactionInternalType[]
			{
				new TransactionInternalType{TransactionInternalTypeId = 1, Name = "Payment"},
				new TransactionInternalType{TransactionInternalTypeId = 2, Name = "Receive payment"},
				new TransactionInternalType{TransactionInternalTypeId = 3, Name = "Receive refund"},
				new TransactionInternalType{TransactionInternalTypeId = 4, Name = "Profit"},
			});


			modelBuilder.Entity<TransactionCoinType>().HasData(new TransactionCoinType[]
			{
				new TransactionCoinType{TransactionCoinTypeId = 1, Name = "Get coin"},
				new TransactionCoinType{TransactionCoinTypeId = 2, Name = "Use coin"},
				new TransactionCoinType{TransactionCoinTypeId = 3, Name = "Refund"}
			});

			modelBuilder.Entity<ProductStatus>().HasData(new ProductStatus[]
			{
				new ProductStatus{ProductStatusId = 1, ProductStatusName = "Active"},
				new ProductStatus{ProductStatusId = 2, ProductStatusName = "Ban"},
				new ProductStatus{ProductStatusId = 3, ProductStatusName = "Hide"},
			});

			modelBuilder.Entity<OrderStatus>().HasData(new OrderStatus[]
			{
				new OrderStatus{OrderStatusId = 1, Name = "Wait for customer confirmation"},
				new OrderStatus{OrderStatusId = 2, Name = "Confirmed"},
				new OrderStatus{OrderStatusId = 3, Name = "Complaint"},
				new OrderStatus{OrderStatusId = 4, Name = "Seller refunded"},
				new OrderStatus{OrderStatusId = 5, Name = "Dispute"},
				new OrderStatus{OrderStatusId = 6, Name = "Reject Complaint"},
				new OrderStatus{OrderStatusId = 7, Name = "Seller violates"},
			});

			modelBuilder.Entity<WithdrawTransactionStatus>().HasData(new WithdrawTransactionStatus[]
			{
				new WithdrawTransactionStatus{WithdrawTransactionStatusId = 1, Name = "In processing"},
				new WithdrawTransactionStatus{WithdrawTransactionStatusId = 2, Name = "Paid"},
				new WithdrawTransactionStatus{WithdrawTransactionStatusId = 3, Name = "Reject"},
			});

			modelBuilder.Entity<User>().HasData(new User[]
			{
				new User(){UserId = 1, RoleId = 1, Username = "admin", Password = "123", Fullname="Admin", Avatar =  "", Status = true, TwoFactorAuthentication = false, AccountBalance = 0, SignInGoogle = false, IsConfirm = true }
			});


			modelBuilder.Entity<Bank>().HasData(new Bank[]
			{
				new Bank(){BankId = 458761, BankName = "TNHH MTV HSBC Việt Nam (HSBC)", BankCode = "HSBC", isActivate = true},
				new Bank(){BankId = 970403, BankName = "Sacombank (STB)", BankCode = "STB", isActivate = true},
				new Bank(){BankId = 970405, BankName = "Nông nghiệp và Phát triển nông thôn (VBA)", BankCode = "VBA", isActivate = true},
				new Bank(){BankId = 970407, BankName = "Kỹ Thương (TCB)", BankCode = "TCB", isActivate = true},
				new Bank(){BankId = 970415, BankName = "Công Thương Việt Nam (VIETINBANK)", BankCode = "VIETINBANK", isActivate = true},
				new Bank(){BankId = 970418, BankName = "Đầu tư và phát triển (BIDV)", BankCode = "BIDV", isActivate = true},
				new Bank(){BankId = 970422, BankName = "Quân đội (MB)", BankCode = "MB", isActivate = true},
				new Bank(){BankId = 970423, BankName = "Tiên Phong (TPB)", BankCode = "TPB", isActivate = true},
				new Bank(){BankId = 970426, BankName = "Hàng hải (MSB)", BankCode = "MSB", isActivate = true},
				new Bank(){BankId = 970432, BankName = "Việt Nam Thinh Vượng (VPB)", BankCode = "VPB", isActivate = true},
				new Bank(){BankId = 970436, BankName = "Ngoại thương Việt Nam (VCB)", BankCode = "VCB", isActivate = true},
				new Bank(){BankId = 970441, BankName = "Quốc tế (VIB)", BankCode = "VIB", isActivate = true},
				new Bank(){BankId = 970443, BankName = "Sài Gòn Hà Nội (SHB)", BankCode = "SHB", isActivate = true},
				new Bank(){BankId = 970449, BankName = "Bưu điện Liên Việt (LPB)", BankCode = "LPB", isActivate = true},
			});

			modelBuilder.Entity<BusinessFee>().HasData(new BusinessFee[]
			{
				new BusinessFee() {BusinessFeeId = 1, Fee = 5, StartDate = DateTime.Now},
			});

			modelBuilder.Entity<FeedbackBenefit>().HasData(new FeedbackBenefit[]
			{
				new FeedbackBenefit{FeedbackBenefitId = 1, Coin = 100, StartDate = DateTime.Now},
			});
		}

	}
}
