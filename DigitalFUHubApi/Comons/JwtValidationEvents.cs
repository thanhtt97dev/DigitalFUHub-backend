﻿using BusinessObject;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;

namespace DigitalFUHubApi.Comons
{
    public class JwtValidationEvents : JwtBearerEvents
	{
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

			/*
			// Get cookie
			var jwtId = string.Empty;
			var httpContextAccessor = context.HttpContext.RequestServices.GetRequiredService<IHttpContextAccessor>();
			if (httpContextAccessor != null)
			{
				jwtId = httpContextAccessor.HttpContext.Request.Cookies["_tid"];
			}
			*/

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
			return base.TokenValidated(context);
		}

		//other events
		/*
		public override Task AuthenticationFailed(AuthenticationFailedContext context)
		{
			return base.AuthenticationFailed(context);
		}

		public override Task Challenge(JwtBearerChallengeContext context)
		{
			return base.Challenge(context);
		}

		public override Task Forbidden(ForbiddenContext context)
		{
			return base.Forbidden(context);
		}

		public override Task MessageReceived(MessageReceivedContext context)
		{
			return base.MessageReceived(context);
		}
		*/
	}
}
