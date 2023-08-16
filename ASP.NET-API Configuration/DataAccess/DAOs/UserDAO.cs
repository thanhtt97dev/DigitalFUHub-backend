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

		public User GetUserByEmailAndPassword(string? email, string? password)
		{
			using (ApiContext context = new ApiContext())
			{
				var user = context.User.Include(x => x.Role).FirstOrDefault(x => x.Email == email && x.Password == password);
				return user;
			}
		}

		internal User? GetUserByRefreshToken(string? refreshTokenStr)
		{
			using (ApiContext context = new ApiContext())
			{
				var refreshToken = RefreshTokenDAO.Instance.GetRefreshToken(refreshTokenStr);
				if (refreshToken == null) return null;
				var user = context.User.Include(x => x.Role).FirstOrDefault(x => x.UserId == refreshToken.UserId);
				return user;
			}
		}
	}
}
