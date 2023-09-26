using BusinessObject.DataTransfer;
using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
		public virtual DbSet<DepositeTransactionBill> DepositeTransactionBill { get; set; } = null!;
		public virtual DbSet<WithdrawTransaction> WithdrawTransaction { get; set; } = null!;
		public virtual DbSet<WithdrawTransactionBill> WithdrawTransactionBill { get; set; } = null!;
		public virtual DbSet<Transaction> Transaction { get; set; } = null!;
        public virtual DbSet<TransactionType> TransactionType { get; set; } = null!;
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
        public virtual DbSet<Media> Media { get; set; } = null!;
        public virtual DbSet<MediaType> MediaType { get; set; } = null!;
        public virtual DbSet<Coupon> Coupon { get; set; } = null!;
        public virtual DbSet<Order> Order { get; set; } = null!;
        public virtual DbSet<OrderStatus> OrderStatus { get; set; } = null!;
        public virtual DbSet<OrderCoupon> OrderCoupon { get; set; } = null!;
        public virtual DbSet<Feedback> Feedback { get; set; } = null!;
        public virtual DbSet<Category> Category { get; set; } = null!;
        public virtual DbSet<Cart> Cart { get; set; } = null!;
        public virtual DbSet<AssetInformation> AssetInformation { get; set; } = null!;
        public virtual DbSet<PlatformFee> PlatformFee { get; set; } = null!;

        #endregion



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cart>().HasKey(x => new { x.UserId, x.ProductTypeId });
            modelBuilder.Entity<OrderCoupon>().HasKey(x => new { x.OrderId, x.CouponId });
            modelBuilder.Entity<OrderCoupon>().
                HasOne(x => x.Order)
                .WithMany(x => x.OrderCoupons)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Transaction>().
                HasOne(x => x.User)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);

			modelBuilder.Entity<Role>().HasData(new Role[]
			{
				new Role{RoleId = 1, RoleName = "Admin"},
				new Role{RoleId = 2, RoleName = "Customer"},
				new Role{RoleId = 3, RoleName = "Seller"},
			});

			modelBuilder.Entity<MediaType>().HasData(new MediaType[]
			{
                new MediaType{MediaTypeId = 1, Name = "Product"},
				new MediaType{MediaTypeId = 2, Name = "Feedback"},
			});

			modelBuilder.Entity<TransactionType>().HasData(new TransactionType[]
			{
				new TransactionType{TransactionTypeId = 1, Name = "Payment"},
				new TransactionType{TransactionTypeId = 2, Name = "Receive payment"},
				new TransactionType{TransactionTypeId = 3, Name = "Receive refund"},
				new TransactionType{TransactionTypeId = 4, Name = "Profit"},
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
				new OrderStatus{OrderStatusId = 4, Name = "Reject Complaint"},
				new OrderStatus{OrderStatusId = 5, Name = "Accept Complaint"},
			});

			
		}

	}
}
