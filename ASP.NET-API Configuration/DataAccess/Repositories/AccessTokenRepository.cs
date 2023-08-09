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
	public class AccessTokenRepository : IAccessTokenRepository
	{
		public Task AddAccessTokenAsync(AccessToken accessToken) => AccessTokenDAO.Instance.AddAccessTokenAsync(accessToken);

		public Task RemoveAllAccessTokenByUserIdAsync(string? userId) => AccessTokenDAO.Instance.RemoveAllAccessTokenByUserIdAsync(userId);
	}
}
