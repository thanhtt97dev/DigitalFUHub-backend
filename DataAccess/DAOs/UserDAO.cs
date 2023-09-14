using BusinessObject;
using DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
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

		public User? GetUserByUsernameAndPassword(string? username, string? password)
		{
			using (ApiContext context = new ApiContext())
			{
				var user = context.User.Include(x => x.Role).FirstOrDefault(x => x.Username == username && x.Password == password);
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

		internal List<User> GetUsers(UserRequestDTO userDTO)
		{
			using (ApiContext context = new ApiContext())
			{
				var list = context.User.Include(x => x.Role).Where(x => x.Email.Contains(userDTO.Email)).ToList();
				if(userDTO.RoleId != 0) list = list.Where(x => x.RoleId == userDTO.RoleId).ToList();
				if(userDTO.Status != -1) list = list.Where(x => x.Status == (userDTO.Status == 1)).ToList();
				return list;
			}
		}

		internal User? GetUserById(int id)
		{
			using (ApiContext context = new ApiContext())
			{
				return context.User.Include(x => x.Role).FirstOrDefault(x => x.UserId == id);
			}
		}

		internal async Task EditUserInfo(int id, User userUpdate)
		{
			using (ApiContext context = new ApiContext())
			{
				var user = await context.User.FirstAsync(x => x.UserId == id);
				user.RoleId = userUpdate.RoleId;	
				user.Status = userUpdate.Status;
				await context.SaveChangesAsync();
			}
		}

		internal void Update2FA(int id)
		{
			using (ApiContext context = new ApiContext())
			{
				var user =  context.User.FirstOrDefault(x => x.UserId == id);
				if (user == null) throw new Exception("User not existed!");
				user.TwoFactorAuthentication = !user.TwoFactorAuthentication;
				context.SaveChanges();
			}
		}
	}
}
