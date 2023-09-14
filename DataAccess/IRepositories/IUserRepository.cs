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
		User? GetUserByEmailAndPassword(string? email, string? password);
		User? GetUserByEmail(string? email);

		User? GetUserByRefreshToken(string? refreshTokenId);

		List<User> GetUsers(UserRequestDTO user);

		User? GetUserById(int id);

		Task EditUserInfo(int id, User user);

		void Update2FA(int id);

		void AddUser(User user);
	}
}
