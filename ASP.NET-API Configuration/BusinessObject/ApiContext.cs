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
		public ApiContext()
		{
		}

		public ApiContext(DbContextOptions options) : base(options)
		{
		}

		public virtual DbSet<Role> Role { get; set; } = null!;
		public virtual DbSet<User> User { get; set; } = null!;

		public virtual DbSet<AccessToken> AccessToken { get; set; } = null!;
		public virtual DbSet<RefreshToken> RefreshToken { get; set; } = null!;

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			string projectName = "ServerAPI";
			var directory = Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().LastIndexOf("\\")) + "\\" + projectName;
			var conf = new ConfigurationBuilder()
				.SetBasePath(directory)
				.AddJsonFile("appsettings.json", true, true)
				.Build();
			if (!optionsBuilder.IsConfigured)
			{
			optionsBuilder.UseSqlServer(conf.GetConnectionString("DB") ?? string.Empty);
			}
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Role>().HasData(new Role[]
			{
				new Role{RoleId = 1, RoleName="Admin"},
				new Role{RoleId = 2, RoleName="User"}
			});
		}
	}
}
