using Microsoft.EntityFrameworkCore;
using BusinessObject;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using AutoMapper;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using FuMarketAPI.Services;
using FuMarketAPI.Comons;
using FuMarketAPI.Hubs;
using FuMarketAPI.Managers;
using AspNetCoreRateLimit;
using Quartz;
using FuMarketAPI.Jobs;

namespace FuMarketAPI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			// Add database
			builder.Services.AddDbContext<ApiContext>(opts =>
			{
				opts.UseSqlServer(builder.Configuration.GetConnectionString("DB") ?? string.Empty);
			});

			// Add HttpContextAccessor 
			builder.Services.AddHttpContextAccessor();

			//Add JWT
			builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options =>
				{
					options.TokenValidationParameters = new JwtValidationParameters();
					options.Events = new JwtValidationEvents();
				});

			//Add for more Policy Authorization
			builder.Services.AddAuthorization(options =>
			{
				options.AddPolicy("AdminOnly", policy =>
					policy.RequireRole("Admin"));
			});

			//Add Cors
			builder.Services.AddCors(options =>
			{
				options.AddDefaultPolicy(policy =>
				{
					policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
				});
			});


			// Disable auto model state validate
			builder.Services.Configure<ApiBehaviorOptions>(opts =>
			{
				opts.SuppressModelStateInvalidFilter = true;
			});

			//Add Auto Mapper
			var configAutoMapper = new MapperConfiguration(config =>
			{
				config.AddProfile(new AutoMapperProfile());
			});
			var mapper = configAutoMapper.CreateMapper();
			builder.Services.AddSingleton(mapper);

			// Remove cycle object's data in json respone
			builder.Services.AddControllers().AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
			});

			//Add DI 
			builder.Services.AddSingleton<IUserRepository, UserRepository>();
			builder.Services.AddSingleton<IRefreshTokenRepository, RefreshTokenRepository>();
			builder.Services.AddSingleton<IAccessTokenRepository, AccessTokenRepository>();
			builder.Services.AddSingleton<IRoleRepository, RoleRepository>();
			builder.Services.AddSingleton<INotificationRepositiory, NotificationRepositiory>();
			builder.Services.AddSingleton<IStorageRepository, StorageRepository>();
            builder.Services.AddSingleton<IReportRepository, ReportRepository>();
			builder.Services.AddSingleton<ITwoFactorAuthenticationRepository, TwoFactorAuthenticationRepository>();
            builder.Services.AddSingleton<IChatRepository, ChatRepository>();
			builder.Services.AddSingleton<IBankRepository, BankRepository>();

            builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();

			builder.Services.AddSingleton<JwtTokenService>();
			builder.Services.AddSingleton<HubConnectionService>();
			builder.Services.AddSingleton<TwoFactorAuthenticationService>();
			builder.Services.AddSingleton<MailService>();
			builder.Services.AddSingleton<MbBankService>();	
			builder.Services.AddSingleton<StorageService>();	

			//Add SignalR
			builder.Services.AddSignalR();

			//Add rate limit request
			builder.Services.Configure<IpRateLimitOptions>(options =>
			{
				options.EnableEndpointRateLimiting = true;
				options.StackBlockedRequests = false;
				options.HttpStatusCode = 429;
				options.RealIpHeader = "X-Real-IP";
				options.ClientIdHeader = "X-ClientId";
				options.GeneralRules = Constants.RateLimitRules;
			});
			builder.Services.AddMemoryCache();
			builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
			builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
			builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
			builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

			// add job scheduler get history transaction bank
			builder.Services.AddQuartz(q =>
			{
				var jobKeyGetHistoryTransaction = new JobKey("GetHistoryTransactionJob");
				q.AddJob<HistoryTransactionMbBankJob>(opts => opts.WithIdentity(jobKeyGetHistoryTransaction));
				q.AddTrigger(opts => opts
					.ForJob(jobKeyGetHistoryTransaction)
					.StartNow()
					.WithSimpleSchedule(x =>
						x.WithIntervalInMinutes(1)
						.RepeatForever()
						)
					);
			});

			builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

			var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}


			app.UseIpRateLimiting();

			app.UseCors();

			app.UseAuthentication();

			app.UseRouting();

			app.UseAuthorization();

			//Mapping hubs
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapHub<NotificationHub>("/notificationHub");
                endpoints.MapHub<ChatHub>("/chatHub");
            });

			// Add https
			//app.UseHttpsRedirection();

			app.MapControllers();

			app.Run();
		}
	}
}