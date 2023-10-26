using BusinessObject;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;

namespace DigitalFUHubApi.Comons
{
	public class JwtBearerValidationEvents : JwtBearerEvents
	{

		public override Task MessageReceived(MessageReceivedContext context)
		{
			// If the request is for our hub...
			var path = context.HttpContext.Request.Path.ToString();
			if (path.Contains("/hubs/"))
			{
				var accessToken = context.Request.Query["access_token"];
				context.Token = accessToken;
			}
			return Task.CompletedTask;
		}

		public override Task TokenValidated(TokenValidatedContext context)
		{
			// Retrieve user's info from token claims
			string? jwtId = string.Empty;
			string? userIdStr = string.Empty;
			if (context.SecurityToken is JwtSecurityToken jwtSecurityToken)
			{
				jwtId = jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Jti)?.Value ?? string.Empty;
				userIdStr = jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub)?.Value ?? string.Empty;
			}

			int userId;
			int.TryParse(userIdStr, out userId);

			var dbContext = context.HttpContext.RequestServices.GetRequiredService<DatabaseContext>();
			var user = dbContext.User.FirstOrDefault(x => x.UserId == userId);
			if (user == null || !user.Status)
			{
				context.Fail("Unauthorized");
				return base.TokenValidated(context);
			}

			var accessToken = dbContext.AccessToken.FirstOrDefault(x => x.JwtId == jwtId && x.IsRevoked == false);
			if (accessToken == null)
			{
				//Hanlde revoke all token of this user
				var tokens = dbContext.AccessToken.Where(x => x.UserId == userId && x.IsRevoked == false).ToList();
				tokens.ForEach((token) => { token.IsRevoked = true; });
				dbContext.SaveChanges();

				context.Fail("Unauthorized");
				return base.TokenValidated(context);
			}

			JwtSecurityToken? securityToken = context.SecurityToken as JwtSecurityToken;
			if (securityToken == null)
			{
				context.Fail("Unauthorized");
			}
			string algorithm = securityToken?.Header?.Alg ?? "";

			if (!string.Equals(algorithm, "HS512", StringComparison.OrdinalIgnoreCase))
			{
				context.Fail("Unauthorized");
			}
			return base.TokenValidated(context);
		}

	}
}
