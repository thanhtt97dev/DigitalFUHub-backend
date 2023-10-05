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
		public User? GetUserById(long id) => UserDAO.Instance.GetUserById(id);

		public void EditUserInfo(int id, User user) => UserDAO.Instance.EditUserInfo(id,user);

		public void Update2FA(int id) => UserDAO.Instance.Update2FA(id);

		public User? GetUserByEmail(string? email) =>  UserDAO.Instance.GetUserByEmail(email);

		public void AddUser(User user) =>  UserDAO.Instance.AddUser(user);

		public User? GetUserByUsername(string username) =>  UserDAO.Instance.GetUserByUsername(username);

		public bool IsExistUsernameOrEmail(string username, string email) =>  UserDAO.Instance.IsExistUsernameOrEmail(username, email);

		public User? GetUser(string email, string username, string fullname) =>  UserDAO.Instance.GetUser(email, username, fullname);

		public void UpdateUser(User user) => UserDAO.Instance.UpdateUser(user);
		public void UpdateBalance(long id, long newAccountBalance)
        {
			var user = UserDAO.Instance.GetUserById(id);
			if (user == null) {
				throw new NullReferenceException("Could not find user with ID = " + id);
			}
			user.AccountBalance = newAccountBalance;

             UserDAO.Instance.UpdateUser(user);
        }
		
	}
}
