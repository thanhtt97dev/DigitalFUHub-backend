﻿using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using AutoMapper;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Services;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Hubs;
using AspNetCoreRateLimit;
using Quartz;
using DigitalFUHubApi.Jobs;
using BusinessObject;
using DigitalFUHubApi.Managers.Repositories;
using DigitalFUHubApi.Managers.IRepositories;

namespace DigitalFUHubApi
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
			builder.Services.AddDbContext<DatabaseContext>(opts =>
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
					options.Events = new JwtBearerValidationEvents();
				});

			//Add for more Policy Authorization
			builder.Services.AddAuthorization(options =>
			{
				options.AddPolicy("Admin", policy =>
					policy.RequireRole("Admin"));
				options.AddPolicy("Seller", policy =>
					policy.RequireRole("Seller"));
				options.AddPolicy("Customer", policy =>
					policy.RequireRole("Customer"));
				options.AddPolicy("Customer,Seller", policy =>
					policy.RequireRole("Customer", "Seller"));
			});

			//Add Cors
			builder.Services.AddCors(options =>
			{
				options.AddDefaultPolicy(policy =>
				{
					policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
				});
			});
			/*
			builder.Services.AddCors(options =>
			{
				options.AddDefaultPolicy(policy =>
				{
					policy.WithOrigins("http://localhost:3000").AllowCredentials().AllowAnyHeader().AllowAnyMethod();
					policy.WithOrigins("http://localhost:4000").AllowCredentials().AllowAnyHeader().AllowAnyMethod();
					policy.WithOrigins("http://52.187.34.218:3000").AllowCredentials().AllowAnyHeader().AllowAnyMethod();
					policy.WithOrigins("http://52.187.34.218:4000").AllowCredentials().AllowAnyHeader().AllowAnyMethod();
				});
			});
			*/

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
			builder.Services.AddSingleton<IReportRepository, ReportRepository>();
			builder.Services.AddSingleton<ITwoFactorAuthenticationRepository, TwoFactorAuthenticationRepository>();
			builder.Services.AddSingleton<IConversationRepository, ConversationRepository>();
			builder.Services.AddSingleton<IBankRepository, BankRepository>();
			builder.Services.AddSingleton<IProductRepository, ProductRepository>();
			builder.Services.AddSingleton<ICategoryRepository, CategoryRepository>();
			builder.Services.AddSingleton<IShopRepository, ShopRepository>();
			builder.Services.AddSingleton<IFeedbackRepository, FeedbackRepository>();
			builder.Services.AddSingleton<ICartRepository, CartRepository>();
			builder.Services.AddSingleton<IOrderRepository, OrderRepository>();
			builder.Services.AddSingleton<IAssetInformationRepository, AssetInformationRepository>();
			builder.Services.AddSingleton<ICouponRepository, CouponRepository>();
			builder.Services.AddSingleton<IBusinessFeeRepository, BusinessFeeRepositoty>();
			builder.Services.AddSingleton<ITransactionInternalRepository, TransactionInternalRepository>();
			builder.Services.AddSingleton<ITransactionCoinRepository, TransactionCoinRepository>();
			builder.Services.AddSingleton<IUserConversationRepository, UserConversationRepository>();
            builder.Services.AddSingleton<IWishListRepository, WishListRepository>();
			builder.Services.AddSingleton<IReportProductRepository, ReportProductRepository>();
            builder.Services.AddSingleton<IReasonReportProductRepository, ReasonReportProductRepository>();
            builder.Services.AddSingleton<ISliderRepository, SliderRepository>();
            builder.Services.AddSingleton<IFeedbackBenefitRepository, FeedbackBenefitRepository>();
			builder.Services.AddSingleton<IShopRegisterFeeRepository, ShopRegisterFeeRepository>();

			builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();
			builder.Services.AddSingleton<IFinanceTransactionManager, FinanceTransactionManager>();

			builder.Services.AddSingleton<JwtTokenService>();
			builder.Services.AddSingleton<HubService>();
			builder.Services.AddSingleton<TwoFactorAuthenticationService>();
			builder.Services.AddSingleton<MailService>();
			builder.Services.AddSingleton<MbBankService>();
			builder.Services.AddSingleton<AzureStorageAccountService>();
			builder.Services.AddSingleton<OpticalCharacterRecognitionService>();

			//Add SignalR
			builder.Services.AddSignalR(c =>
			{
				c.EnableDetailedErrors = true;
				c.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
				c.KeepAliveInterval = TimeSpan.FromSeconds(15);
			});

			//Add rate limit request
			builder.Services.Configure<IpRateLimitOptions>(options =>
			{
				options.EnableEndpointRateLimiting = true;
				options.StackBlockedRequests = false;
				options.HttpStatusCode = 429;
				options.RealIpHeader = "X-Real-IP";
				options.ClientIdHeader = "X-ClientId";
				options.GeneralRules =
					new List<RateLimitRule>()
					{
						new RateLimitRule
						{
							Endpoint = "*",
							Period = "10s",
							Limit = 10,
						}
					};
			});
			builder.Services.AddMemoryCache();
			builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
			builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
			builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
			builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();


			// add job scheduler

			builder.Services.AddQuartz(configure =>
			{
				configure.UseMicrosoftDependencyInjectionJobFactory();
			});

			builder.Services.AddQuartzHostedService(options =>
			{
				options.WaitForJobsToComplete = true;
			});
			
			builder.Services.AddQuartz(configure =>
			{
				var jobKeyGetSessionIdMbBankJob = new JobKey("GetSessionIdMbBankJob");
				configure.AddJob<GetSessionIdMbBankJob>(jobKeyGetSessionIdMbBankJob)
						 .AddTrigger(trigger =>
							trigger.ForJob(jobKeyGetSessionIdMbBankJob)
							.StartNow()
							.WithSimpleSchedule(schedule => 
								schedule.WithIntervalInMinutes(25).RepeatForever()
								)
							);

				var jobKeyHistoryTransactionMbBankJob = new JobKey("HistoryTransactionMbBankJob");
				configure.AddJob<HistoryTransactionMbBankJob>(jobKeyHistoryTransactionMbBankJob)
						 .AddTrigger(trigger =>
							trigger.ForJob(jobKeyHistoryTransactionMbBankJob)
							.StartNow()
							.WithSimpleSchedule(schedule => 
								schedule.WithIntervalInSeconds(61).RepeatForever()
								)
							);

				var jobKeyHistoryDepositTransactionMbBankJob = new JobKey("HistoryDepositTransactionMbBankJob");
				configure.AddJob<HistoryDepositTransactionMbBankJob>(jobKeyHistoryDepositTransactionMbBankJob)
						 .AddTrigger(trigger =>
							trigger.ForJob(jobKeyHistoryDepositTransactionMbBankJob)
							.StartNow()
							.WithSimpleSchedule(schedule => 
								schedule.WithIntervalInSeconds(111).RepeatForever()
								)
							);

				var jobKeyHistoryWithdrawTransactionMbBankJob = new JobKey("HistoryWithdrawTransactionMbBankJob");
				configure.AddJob<HistoryWithdrawTransactionMbBankJob>(jobKeyHistoryWithdrawTransactionMbBankJob)
						 .AddTrigger(trigger =>
							trigger.ForJob(jobKeyHistoryWithdrawTransactionMbBankJob)
							.StartNow()
							.WithSimpleSchedule(schedule => 
								schedule.WithIntervalInSeconds(183).RepeatForever()
								)
							);


				var jobKeyOrderStatusJob = new JobKey("OrderStatusJob");
				configure.AddJob<OrderStatusJob>(jobKeyOrderStatusJob)
						 .AddTrigger(trigger =>
							trigger.ForJob(jobKeyOrderStatusJob)
							.StartNow()
							.WithSimpleSchedule(schedule => 
								schedule.WithIntervalInMinutes(30).RepeatForever()
								)
							);

				var jobKeyUpdateStatusRequestDepositMoneyToExpiredJob = new JobKey("UpdateStatusRequestDepositMoneyToExpired");
				configure.AddJob<UpdateStatusRequestDepositMoneyToExpired>(jobKeyUpdateStatusRequestDepositMoneyToExpiredJob)
						 .AddTrigger(trigger =>
							trigger.ForJob(jobKeyUpdateStatusRequestDepositMoneyToExpiredJob)
							.StartNow()
							.WithSimpleSchedule(schedule =>
								schedule.WithIntervalInHours(24).RepeatForever()
								)
							);

				configure.UseMicrosoftDependencyInjectionJobFactory();
			});


			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseIpRateLimiting();

			app.UseRouting();

			app.UseCors();

			app.UseAuthentication();

			app.UseAuthorization();

			//Mapping hubs
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapHub<UserOnlineStatusHub>("/hubs/userOnlineStatus");
				endpoints.MapHub<NotificationHub>("/hubs/notification");
				endpoints.MapHub<ChatHub>("/hubs/chat");
			});

			// Add https
			//app.UseHttpsRedirection();

			app.MapControllers();

			app.Run();
		}
	}
}