using BusinessObject;
using BusinessObject.Entities;
using Comons;
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

		internal void EditUserInfo(User userUpdate)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var user = context.User.First(x => x.UserId == userUpdate.UserId);
				user.Avatar = userUpdate.Avatar ?? user.Avatar;
				user.Fullname = userUpdate.Fullname ?? user.Fullname;
				user.Username = userUpdate.Username ?? user.Username;
				user.Password = userUpdate.Password ?? user.Password;
				context.SaveChanges();
			}
		}

		internal void UpdateSettingPersonalInfo(User userUpdate)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var user = context.User.First(x => x.UserId == userUpdate.UserId);

				user.Avatar = user.Avatar != userUpdate.Avatar ? userUpdate.Avatar : user.Avatar;
				user.Fullname = user.Fullname != userUpdate.Fullname ? userUpdate.Fullname : user.Fullname;

				context.SaveChanges();
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

		internal User? GetUserByEmail(string? email)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				if (email == null) return null;
				return context.User.Include(x => x.Role).FirstOrDefault(x => x.Email.ToLower() == email.ToLower());
			}
		}

		internal void AddUser(User user)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				try
				{
					context.User.Add(user);
					context.SaveChanges();
				}
				catch (Exception e)
				{
					throw new Exception(e.Message);
				}
			}
		}

		internal User? GetUserByUsername(string username)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.User.Include(x => x.Role).FirstOrDefault(x => x.Username.ToLower() == username.ToLower());
			}
		}

		internal bool IsExistUsernameOrEmail(string username, string email)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.User.Any(x => x.Username.ToLower() == username.ToLower() || x.Email.ToLower() == email.ToLower());
			}
		}

		internal User? GetUser(string email, string username, string fullname)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.User.FirstOrDefault(x => x.Username == username && x.Email == email && x.Fullname == fullname);
			}
		}

		internal void UpdateUser(User user)
		{
			try
			{
				using (DatabaseContext context = new DatabaseContext())
				{
					context.User.Update(user);
					context.SaveChanges();
				}
			}
			catch (Exception e)
			{
				throw new Exception(e.Message);
			}
		}

		internal int GetNumberUserWithCondition(long userId, string email, string fullName, int roleId, int status)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var users = context.User
							.Include(x => x.Role)
							.Where(x =>
								x.Email.Contains(email) && x.Fullname.Contains(fullName) &&
								x.RoleId != Constants.ADMIN_ROLE &&
								(userId == 0 ? true : x.UserId == userId) &&
								(roleId == 0 ? true : x.RoleId == roleId) &&
								(status == 0 ? true : x.Status == (status == 1))
								).Count();
				return users;
			}
		}

		internal List<User> GetUsers(long userId, string email, string fullName, int roleId, int status, int page)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var users = context.User
							.Include(x => x.Role)
							.Where(x =>
								x.Email.Contains(email) && x.Fullname.Contains(fullName) &&
								x.RoleId != Constants.ADMIN_ROLE &&
								(userId == 0 ? true : x.UserId == userId) &&
								(roleId == 0 ? true : x.RoleId == roleId) &&
								(status == 0 ? true : x.Status == (status == 1))
								)
								.Skip((page - 1) * Constants.PAGE_SIZE)
							   .Take(Constants.PAGE_SIZE)
							   .ToList();
				return users;
			}
		}

		internal User? GetUserInfo(int id)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var user = context.User.FirstOrDefault(x => x.UserId == id);
				if (user == null) return null;

				return user;
			}
		}

		internal bool CheckUsersExisted(List<long> userIds)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.User.Any(x => userIds.Contains(x.UserId));
			}
		}

		internal void UpdateUserOnlineStatus(long userId, bool isOnline)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var user = context.User.FirstOrDefault(x => x.UserId == userId);
				if (user == null) return;
				if (user.Status == false) return;
				if (isOnline && user.IsOnline == isOnline) return;
				if (!isOnline && user.IsOnline == isOnline) return;
				if (isOnline)
				{
					user.IsOnline = true;
				}
				else
				{
					user.IsOnline = false;
					user.LastTimeOnline = DateTime.Now;
				}
				context.SaveChanges();
			}
		}

		internal string GenerateRandomUsername(string email)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				string chars = "qwertyuiopasdfghjklzxcvbnm123456789";
				string firstPartEmail = email.Split("@")[0];
				while (true)
				{
					string firstFourCharacter = "";
					Random rd = new Random();
					if (firstPartEmail.Length > 4)
					{
						firstFourCharacter = firstPartEmail.Substring(0, 4);
					}
					else
					{
						firstFourCharacter = firstPartEmail;
					}
					string username = firstFourCharacter;
					int numberCharConcat = rd.Next(4 - firstFourCharacter.Length == 0 ? 2 : 4 + 2 - firstFourCharacter.Length, 9);
					for (int i = 0; i < numberCharConcat; i++)
					{
						username += chars[rd.Next(0, chars.Length)];
					}

					if (!context.User.Any(x => x.Username == username))
					{
						return username;
					}

				}

			}
		}

		internal User? GetUserByUserNameOtherUserId(long userId, string userName)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var result = (from user in context.User
							  where user.UserId != userId
							  &&
							  user.Username.ToUpper().Trim().Equals(userName.ToUpper().Trim())
							  select new User { }).FirstOrDefault();

				return result;
			}
		}

		internal void ActiveUserNameAndPassword(long userId, string userName, string password)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var userFind = context.User.FirstOrDefault(x => x.UserId == userId);

				if (userFind == null) throw new ArgumentNullException(nameof(userFind));

				userFind.Username = userName;
				userFind.Password = password;
				userFind.IsChangeUsername = true;

				context.SaveChanges();
			}
		}

		internal long GetNumberNewUserInCurrentMonth()
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				DateTime now = DateTime.Now;
				return context.User.LongCount(x => x.UserId != Constants.ADMIN_USER_ID && x.CreateDate.Month == now.Month && x.CreateDate.Year == now.Year);
			}
		}

		internal User? GetUserInfoById(long id)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.User
					.Include(x => x.Shop)
					.ThenInclude(x => x.Orders)
					.ThenInclude(x => x.BusinessFee)
					.Include(x => x.Orders)
					.FirstOrDefault(x => x.UserId == id);
			}
		}
	}
}
