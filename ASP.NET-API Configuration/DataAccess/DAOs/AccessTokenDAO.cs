using BusinessObject;
using Microsoft.EntityFrameworkCore;
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

		internal async Task AddAccessTokenAsync(AccessToken accessToken)
		{
			using (ApiContext context = new ApiContext())
			{
				 await context.AccessToken.AddAsync(accessToken);
				context.SaveChanges();
			}
		}

		internal async Task RemoveAllAccessTokenByUserIdAsync(string? userId)
		{
			using (ApiContext context = new ApiContext())
			{
				var accessTokens = await context.AccessToken.Where(x => x.UserId == int.Parse(userId ?? "0")).ToListAsync();
				context.AccessToken.RemoveRange(accessTokens);
				context.SaveChanges();	
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
