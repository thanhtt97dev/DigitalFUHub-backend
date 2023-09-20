using BusinessObject;
using DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
	public interface IUserRepository
	{
		User? GetUserByUsernameAndPassword(string? email, string? password);
		Task<User?> GetUserByEmail(string? email);

		User? GetUserByRefreshToken(string? refreshTokenId);

		List<User> GetUsers(UserRequestDTO user);

		User? GetUserById(int id);

		Task EditUserInfo(int id, User user);

		void Update2FA(int id);

		Task AddUser(User user);
		Task<User?> GetUserByUsername(string username);
		Task<bool> IsExistUsernameOrEmail(string username, string email);
	}
}
