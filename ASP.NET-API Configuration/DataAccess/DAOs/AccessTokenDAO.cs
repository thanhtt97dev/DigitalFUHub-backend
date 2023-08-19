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

		internal async Task<AccessToken?> GetAccessTokenAsync(string? token)
		{
			using (ApiContext context = new ApiContext())
			{
				var accessToken = await context.AccessToken.FirstOrDefaultAsync(x => x.Token == token);
				return accessToken;
			}
		}

		internal void RevokeToken(string jwtId)
		{
			using (ApiContext context = new ApiContext())
			{
				var token = context.AccessToken.FirstOrDefault(x => x.JwtId == jwtId);
				if (token == null) throw new NullReferenceException("AccessToken is not existed!");
				token.isRevoked = true;
				context.SaveChanges();
			}
		}
		
	}
}
