using Microsoft.EntityFrameworkCore;
using BusinessObject;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using AutoMapper;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using ServerAPI.Services;
using System.Security.Claims;

namespace ServerAPI
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

			//Add Odata
			builder.Services.AddControllers()
				.AddOData(options =>
				{
					options.Select().Filter().Count().OrderBy().SetMaxTop(100).Expand()
					.AddRouteComponents("odata", GetEdmModel());
				});



			//Add JWT
			builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(opt =>
				{
					opt.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = false,
						ValidateAudience = false,

						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"] ?? string.Empty)),
						/*
						ValidateIssuer = true,
						ValidIssuer = configuration["JWT:Issuer"],
						ValidateAudience = true,
						ValidAudience = configuration["JWT:Audience"],
						*/
						ValidateLifetime = true,
					};

					//Checking token has been in DB
					opt.Events = new JwtBearerEvents
					{
						OnTokenValidated = async context =>
						{
							var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApiContext>();
							// Retrieve token from headers
							var jwtId = context.SecurityToken.Id;
							// Retrieve user's ID from token claims
							var userIdClaim = context.Principal.FindFirstValue("Id");

							if (!string.IsNullOrEmpty(userIdClaim))
							{
								var accessToken = await dbContext.AccessToken
								.FirstOrDefaultAsync(x => x.UserId == int.Parse(userIdClaim) && x.JwtId == jwtId);

								if (accessToken == null)
								{
									//Remove all token of user request
									IAccessTokenRepository accessTokenRepository = new AccessTokenRepository();
									await accessTokenRepository.RemoveAllAccessTokenByUserIdAsync(userIdClaim);
									context.Fail("Unauthorized");
								}
							}
							await Task.CompletedTask;
						}
					};
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

			// Remove cycle object's data in json respone
			builder.Services.AddControllers().AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
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

			//Add DI 
			builder.Services.AddSingleton<IUserRepository, UserRepository>();
			builder.Services.AddSingleton<IRefreshTokenRepository, RefreshTokenRepository>();
			builder.Services.AddSingleton<IAccessTokenRepository, AccessTokenRepository>();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseCors();

			app.UseAuthentication();

			app.UseAuthorization();

			app.MapControllers();

			app.Run();
		}

		private static IEdmModel GetEdmModel()
		{
			ODataConventionModelBuilder builder = new ODataConventionModelBuilder();

			return builder.GetEdmModel();
		}


	}
}