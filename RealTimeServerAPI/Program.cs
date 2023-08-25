using AutoMapper;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using RealTimeServerAPI.Hubs;
using RealTimeServerAPI.Managers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RealTimeServerAPI.Validations;
using BusinessObject;
using Microsoft.EntityFrameworkCore;

namespace RealTimeServerAPI
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

			//Add SignalR
			builder.Services.AddSignalR();

			

			// Add database
			builder.Services.AddDbContext<ApiContext>(opts =>
			{
				opts.UseSqlServer(builder.Configuration.GetConnectionString("DB") ?? string.Empty);
			});

			//Add Auto Mapper
			var configAutoMapper = new MapperConfiguration(config =>
			{
				config.AddProfile(new AutoMapperProfile());
			});
			var mapper = configAutoMapper.CreateMapper();
			builder.Services.AddSingleton(mapper);

			//Add DI
			builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();
			builder.Services.AddSingleton<INotificationRepositiory, NotificationRepositiory>();

			//Add JWT
			builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(opt =>
				{
					opt.TokenValidationParameters = new JwtValidationParameters();
 					opt.Events = new JwtValidationEvents();
				});


			//Add cors
			builder.Services.AddCors(options =>
			{
				options.AddDefaultPolicy(policy =>
				{
					policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
				});
			});


			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			//Mapping hubs
			app.MapHub<NotificationHub>("/notificationHub");

			app.UseAuthorization();
			app.UseCors();

			app.MapControllers();

			app.Run();
		}
	}
}