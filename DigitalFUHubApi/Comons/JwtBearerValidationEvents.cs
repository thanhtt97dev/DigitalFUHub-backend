using BusinessObject;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text;

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
				var userid = context.Request.Query["userid"];
				context.Token = accessToken;
				context.Request.Headers.Add("session-userid", userid);
			}
			return Task.CompletedTask;
		}

		public override Task TokenValidated(TokenValidatedContext context)
		{
			// Retrieve user's info from token claims
			//string? jwtId = string.Empty;
			string? userIdStr = string.Empty;
			if (context.SecurityToken is JwtSecurityToken jwtSecurityToken)
			{
				if (!string.Equals(jwtSecurityToken.Header.Alg, SecurityAlgorithms.HmacSha512, StringComparison.OrdinalIgnoreCase))
				{
					context.Fail("Unauthorized");
					return base.TokenValidated(context);
				}
				//jwtId = jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Jti)?.Value ?? string.Empty;
				userIdStr = jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub)?.Value ?? string.Empty;
			}

			long userId;
			long.TryParse(userIdStr, out userId);
			/*
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
			*/

			StringValues headerValues;
			context.Request.Headers.TryGetValue("session-userid", out headerValues);
			long.TryParse(headerValues.FirstOrDefault(), out long s_userId);
			if (headerValues.FirstOrDefault() == null || s_userId == 0 || userId == 0 || userId != s_userId)
			{
				context.Fail("Unauthorized");
			}

			return base.TokenValidated(context);
		}

	}
}
