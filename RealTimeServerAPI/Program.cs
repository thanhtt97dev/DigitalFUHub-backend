using AutoMapper;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using RealTimeServerAPI.Hubs;
using RealTimeServerAPI.Managers;

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

			//Add cors
			builder.Services.AddCors(options =>
			{
				options.AddDefaultPolicy(policy =>
				{
					policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
				});
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