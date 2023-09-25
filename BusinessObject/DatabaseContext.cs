using BusinessObject.DataTransfer;
using BusinessObject.Entities2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BusinessObject
{
    public class DatabaseContext : DbContext
    {

        public readonly string connectionString = "server=localhost; database=DBTest; uid=sa; pwd=sa;MultipleActiveResultSets=true";
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
        public virtual DbSet<Transaction> Transaction { get; set; } = null!;
        public virtual DbSet<TransactionType> TransactionType { get; set; } = null!;
        public virtual DbSet<UserConversation> UserConversation { get; set; } = null!;
        public virtual DbSet<Conversation> Conversations { get; set; } = null!;
        public virtual DbSet<Message> Messages { get; set; } = null!;
        public virtual DbSet<Bank> Bank { get; set; } = null!;
        public virtual DbSet<UserBank> UserBank { get; set; } = null!;
        public virtual DbSet<Product> Product { get; set; } = null!;
		public virtual DbSet<ProductType> ProductType { get; set; } = null!;
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
			modelBuilder.Entity<Product>().
                HasOne(x => x.ProductStatus)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.ProductStatusId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Media>().
                HasOne(x => x.Product)
                .WithMany(x => x.Medias)
                .HasForeignKey(x => x.ForeignId)
                .OnDelete(DeleteBehavior.NoAction);
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
        }
    }
}
