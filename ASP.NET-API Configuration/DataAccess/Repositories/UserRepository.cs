using BusinessObject;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
	public class UserRepository : IUserRepository
	{
		public Task<User?> GetUserByEmailAndPasswordAsync(string? email, string? password) => UserDAO.Instance.GetUserByEmailAndPasswordAsync(email, password);

		public Task<User?> GetUserFromRefreshTokenAsync(string? refreshTokenId) => UserDAO.Instance.GetUserFromRefreshTokenAsync(refreshTokenId);
	}
}
