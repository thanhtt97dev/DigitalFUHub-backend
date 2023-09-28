using BusinessObject;
using BusinessObject.Entities;
using DTOs.User;
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
			using (DatabaseContext context = new DatabaseContext())
			{
				var user = context.User.Include(x => x.Role).FirstOrDefault(x => x.Username == username && x.Password == password);
				return user;
			}
		}

		internal User? GetUserByRefreshToken(string? refreshTokenStr)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var refreshToken = RefreshTokenDAO.Instance.GetRefreshToken(refreshTokenStr);
				if (refreshToken == null) return null;
				var user = context.User.Include(x => x.Role).FirstOrDefault(x => x.UserId == refreshToken.UserId);
				return user;
			}
		}

		internal List<User> GetUsers(UserRequestDTO userDTO)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var list = context.User.Include(x => x.Role).Where(x => x.Email.Contains(userDTO.Email)).ToList();
				if (userDTO.RoleId != 0) list = list.Where(x => x.RoleId == userDTO.RoleId).ToList();
				if (userDTO.Status != -1) list = list.Where(x => x.Status == (userDTO.Status == 1)).ToList();
				return list;
			}
		}

		internal User? GetUserById(long id)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.User.Include(x => x.Role).FirstOrDefault(x => x.UserId == id);
			}
		}

		internal async Task EditUserInfo(int id, User userUpdate)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var user = await context.User.FirstAsync(x => x.UserId == id);
                    user.Avatar = userUpdate.Avatar ?? user.Avatar;
					user.Fullname = userUpdate.Fullname ?? user.Fullname;
					user.Username = userUpdate.Username ?? user.Username;
					user.Password = userUpdate.Password ?? user.Password;
                await context.SaveChangesAsync();
			}
		}

		internal void Update2FA(int id)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var user = context.User.FirstOrDefault(x => x.UserId == id);
				if (user == null) throw new Exception("User not existed!");
				user.TwoFactorAuthentication = !user.TwoFactorAuthentication;
				context.SaveChanges();
			}
		}

		internal async Task<User?> GetUserByEmail(string? email)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return await context.User.Include(x => x.Role).FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());
			}
		}

		internal async Task AddUser(User user)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				try
				{
					await context.User.AddAsync(user);
					await context.SaveChangesAsync();
				}
				catch (Exception e)
				{
					throw new Exception(e.Message);
				}
			}
		}

		internal async Task<User?> GetUserByUsername(string username)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return await context.User.Include(x => x.Role).FirstOrDefaultAsync(x => x.Username.ToLower() == username.ToLower());
			}
		}

		internal async Task<bool> IsExistUsernameOrEmail(string username, string email)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return await context.User.AnyAsync(x=> x.Username.ToLower() == username.ToLower() || x.Email.ToLower() == email.ToLower());
			}
		}

		internal async Task<User?> GetUser(string email, string username, string fullname)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return await context.User.FirstOrDefaultAsync(x => x.Username == username && x.Email == email && x.Fullname == fullname);
			}
		}

		internal async Task UpdateUser(User user)
		{
			try
			{
				using (DatabaseContext context = new DatabaseContext())
				{
					context.User.Update(user);
					await context.SaveChangesAsync();
				}
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}
	}
}
