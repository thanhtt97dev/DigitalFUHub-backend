using BusinessObject.DataTransfer;
using BusinessObject.Entities;
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
        public virtual DbSet<Storage> Storage { get; set; } = null!;
        public virtual DbSet<AccessToken> AccessToken { get; set; } = null!;
        public virtual DbSet<RefreshToken> RefreshToken { get; set; } = null!;
        public virtual DbSet<Notification> Notification { get; set; } = null!;
        public virtual DbSet<TwoFactorAuthentication> TwoFactorAuthentication { get; set; } = null!;
        public virtual DbSet<DepositTransaction> DepositTransaction { get; set; } = null!;
        public virtual DbSet<UserConversation> UserConversations { get; set; } = null!;
        public virtual DbSet<Conversation> Conversations { get; set; } = null!;
        public virtual DbSet<Message> Messages { get; set; } = null!;
        public virtual DbSet<SenderConversation> SenderConversations { get; set; } = null!;
        public virtual DbSet<Bank> Bank { get; set; } = null!;
        public virtual DbSet<UserBank> UserBank { get; set; } = null!;
        public virtual DbSet<Product> Product { get; set; } = null!;
        public virtual DbSet<Coupon> Coupon { get; set; } = null!;
        public virtual DbSet<ProductWarehouse> ProductWarehouse { get; set; } = null!;
        public virtual DbSet<Order> Order { get; set; } = null!;
        public virtual DbSet<OrderDetail> OrderDetail { get; set; } = null!;
        public virtual DbSet<Feedback> Feedback { get; set; } = null!;
        public virtual DbSet<ProductImage> ProductImage { get; set; } = null!;
        public virtual DbSet<Category> Category { get; set; } = null!;

        #endregion



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderDetail>().HasKey(x => new { x.OrderId, x.ProductId });
            modelBuilder.Entity<Feedback>().
                HasOne(x => x.Buyer)
                .WithMany(x => x.Feedbacks)
                .HasForeignKey(x => x.BuyerId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<OrderDetail>()
                .HasOne(x => x.Product)
                .WithMany(x => x.OrderDetails)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

        }
    }
}
