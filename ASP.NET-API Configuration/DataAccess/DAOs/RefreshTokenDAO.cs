﻿using BusinessObject;
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

		internal Task AddRefreshTokenAsync(RefreshToken refreshToken)
		{
			using(ApiContext context = new ApiContext()) 
			{
				context.RefreshToken.Add(refreshToken);
				context.SaveChanges();
				return Task.CompletedTask;
			}
		}

		internal async Task<RefreshToken?> GetRefreshToken(string? refreshToken)
		{
			using (ApiContext context = new ApiContext())
			{
				var token = await context.RefreshToken.FirstOrDefaultAsync(x => x.TokenRefresh == refreshToken);
				return token;
			}
		}

		internal async Task RemoveRefreshTokenAysnc(string? refreshTokenId)
		{
			using (ApiContext context = new ApiContext())
			{
				var token = await context.RefreshToken.FirstAsync(x => x.TokenRefresh == refreshTokenId);
				context.RefreshToken.Remove(token);
				context.SaveChanges();
			}
		}
	}
}
