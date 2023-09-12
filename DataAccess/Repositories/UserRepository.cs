using BusinessObject;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
	public class UserRepository : IUserRepository
	{
		public User? GetUserByEmailAndPassword(string? email, string? password) => UserDAO.Instance.GetUserByEmailAndPassword(email, password);
	
		public User? GetUserByRefreshToken(string? refreshTokenId) => UserDAO.Instance.GetUserByRefreshToken(refreshTokenId);

		public List<User> GetUsers(UserRequestDTO userDTO) => UserDAO.Instance.GetUsers(userDTO);
		public User? GetUserById(int id) => UserDAO.Instance.GetUserById(id);

		public Task EditUserInfo(int id, User user) => UserDAO.Instance.EditUserInfo(id,user);

		public void Update2FA(int id) => UserDAO.Instance.Update2FA(id);

	}
}
