using BusinessObject.Entities;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class UserRepository : IUserRepository
	{
		public User? GetUserByUsernameAndPassword(string? username, string? password) => UserDAO.Instance.GetUserByUsernameAndPassword(username, password);
	
		public User? GetUserByRefreshToken(string? refreshTokenId) => UserDAO.Instance.GetUserByRefreshToken(refreshTokenId);

		public List<User> GetUsers(UserRequestDTO userDTO) => UserDAO.Instance.GetUsers(userDTO);
		public User? GetUserById(int id) => UserDAO.Instance.GetUserById(id);

		public Task EditUserInfo(int id, User user) => UserDAO.Instance.EditUserInfo(id,user);

		public void Update2FA(int id) => UserDAO.Instance.Update2FA(id);

		public async Task<User?> GetUserByEmail(string? email) => await UserDAO.Instance.GetUserByEmail(email);

		public async Task AddUser(User user) => await UserDAO.Instance.AddUser(user);

		public async Task<User?> GetUserByUsername(string username) => await UserDAO.Instance.GetUserByUsername(username);

		public async Task<bool> IsExistUsernameOrEmail(string username, string email) => await UserDAO.Instance.IsExistUsernameOrEmail(username, email);

		public async Task<User?> GetUser(string email, string username, string fullname) => await UserDAO.Instance.GetUser(email, username, fullname);

		public async Task UpdateUser(User user) => await UserDAO.Instance.UpdateUser(user);
	}
}
