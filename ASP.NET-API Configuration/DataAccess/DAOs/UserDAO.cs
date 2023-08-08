using BusinessObject;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
	internal class UserDAO
	{
		private static UserDAO? instance;
		private static readonly object instanceLock = new object();

		public static UserDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new UserDAO();
					}
				}
				return instance;
			}
		}

		public async Task<User?> GetUserByEmailAndPasswordAsync(string? email, string? password)
		{
			using (ApiContext context = new ApiContext())
			{
				var user = await context.User.Include(x => x.Role).FirstOrDefaultAsync(x => x.Email == email && x.Password == password);
				return user;
			}
		}

		internal async Task<User?> GetUserFromRefreshTokenAsync(string? refreshTokenId)
		{
			using (ApiContext context = new ApiContext())
			{
				var refreshToken = await RefreshTokenDAO.Instance.GetRefreshToken(refreshTokenId);
				if (refreshToken == null) return null;
				var user = await context.User.Include(x => x.Role).FirstOrDefaultAsync(x => x.UserId == refreshToken.UserId);
				return user;
			}
		}
	}
}
