using BusinessObject;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
	public class RefreshTokenDAO
	{
		private static RefreshTokenDAO? instance;
		private static readonly object instanceLock = new object();

		public static RefreshTokenDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new RefreshTokenDAO();
					}
				}
				return instance;
			}
		}

		internal async Task AddRefreshTokenAsync(RefreshToken refreshToken)
		{
			using(ApiContext context = new ApiContext()) 
			{
				context.RefreshToken.Add(refreshToken);
				await context.SaveChangesAsync();
			}
		}

		internal RefreshToken? GetRefreshToken(string? refreshToken)
		{
			using (ApiContext context = new ApiContext())
			{
				var token = context.RefreshToken.FirstOrDefault(x => x.TokenRefresh == refreshToken);
				return token;
			}
		}

		internal async Task RemoveRefreshTokenAysnc(string? refreshTokenId)
		{
			using (ApiContext context = new ApiContext())
			{
				var token = await context.RefreshToken.FirstAsync(x => x.TokenRefresh == refreshTokenId);
				context.RefreshToken.Remove(token);
				await context.SaveChangesAsync();
			}
		}
	}
}
