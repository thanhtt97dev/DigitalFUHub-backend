using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
	internal class AccessTokenDAO
	{
		private static AccessTokenDAO? instance;
		private static readonly object instanceLock = new object();

		public static AccessTokenDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new AccessTokenDAO();
					}
				}
				return instance;
			}
		}

		internal async Task<AccessToken> AddAccessTokenAsync(AccessToken accessToken)
		{
			using (ApiContext context = new ApiContext())
			{
				EntityEntry<AccessToken> token = await context.AccessToken.AddAsync(accessToken);
				await context.SaveChangesAsync();
				return token.Entity;
			}
		}

		internal async Task RemoveAllAccessTokenUserAsync(string? userId,  string? jwtId)
		{
			using (ApiContext context = new ApiContext())
			{
				List<AccessToken> accessTokens = new List<AccessToken>();
				if(!string.IsNullOrWhiteSpace(userId)) 
				{
					 accessTokens = await context.AccessToken.Where(x => x.UserId == int.Parse(userId)).ToListAsync();
				}
				else
				{
					accessTokens = await context.AccessToken.Where(x => x.JwtId == jwtId).ToListAsync();
				}
				if(accessTokens.Count > 0)
				{
					context.AccessToken.RemoveRange(accessTokens);
					await context.SaveChangesAsync();
				}
			}
		}

		internal async Task<AccessToken?> GetAccessTokenAsync(string? token)
		{
			using (ApiContext context = new ApiContext())
			{
				var accessToken = await context.AccessToken.FirstOrDefaultAsync(x => x.Token == token);
				return accessToken;
			}
		}
	}
}
