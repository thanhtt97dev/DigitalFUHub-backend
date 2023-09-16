using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace BusinessObject
{
	public class ApiContext : DbContext
	{

		public readonly string connectionString = "server=localhost; database=DBTest; uid=sa; pwd=sa;MultipleActiveResultSets=true";
		//public readonly string connectionString = "Server=tcp:fptu-database.database.windows.net,1433;Database=fptu;User ID=fptu;Password=A0336687454a;Trusted_Connection=False;Encrypt=True;";
		
		public ApiContext(){}

		public ApiContext(DbContextOptions options) : base(options){}

        #region DbSet
        public virtual DbSet<Role> Role { get; set; } = null!;
		public virtual DbSet<User> User { get; set; } = null!;
		public virtual DbSet<Storage> Storage { get; set; } = null!;

		public virtual DbSet<AccessToken> AccessToken { get; set; } = null!;
		public virtual DbSet<RefreshToken> RefreshToken { get; set; } = null!;
		public virtual DbSet<Notification> Notification { get; set; } = null!;
		public virtual DbSet<TwoFactorAuthentication> TwoFactorAuthentication { get; set; } = null!;
		public virtual DbSet<DepositTransaction> DepositTransaction { get; set; } = null!;
<<<<<<< HEAD
        public virtual DbSet<UserConversation> UserConversations { get; set; } = null!;
        public virtual DbSet<Conversation> Conversations { get; set; } = null!;
        public virtual DbSet<Message> Messages { get; set; } = null!;
        public virtual DbSet<SenderConversation> SenderConversations { get; set; } = null!;
        #endregion
=======
		public virtual DbSet<Bank> Bank { get; set; } = null!;
		public virtual DbSet<UserBank> UserBank { get; set; } = null!;
>>>>>>> 2a35c2705cc6c4356a418aa88ed8970bc76a292c

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(connectionString);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			
		}
	}
}
