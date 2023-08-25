using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Text;

namespace RealTimeServerAPI.Validations
{
	public class JwtValidationParameters : TokenValidationParameters
	{
		public JwtValidationParameters()
		{
			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", true, true)
				.Build();

			ValidateIssuer = false;
			ValidateAudience = false;
			ValidateIssuerSigningKey = true;
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"] ?? string.Empty));
			/*
			ValidateIssuer = true;
			ValidIssuer = configuration["JWT:Issuer"];
			ValidateAudience = true;
			ValidAudience = configuration["JWT:Audience"];
			*/
			ValidateLifetime = true;
		}
	}
}
