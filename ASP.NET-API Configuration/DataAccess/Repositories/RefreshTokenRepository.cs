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
	public class RefreshTokenRepository : IRefreshTokenRepository
	{
		public Task AddRefreshTokenAsync(RefreshToken refreshToken) => RefreshTokenDAO.Instance.AddRefreshTokenAsync(refreshToken);

		public RefreshToken? GetRefreshToken(string? refreshTokenStr) => RefreshTokenDAO.Instance.GetRefreshToken(refreshTokenStr);

		public Task RemoveRefreshTokenAysnc(string? refreshTokenId) => RefreshTokenDAO.Instance.RemoveRefreshTokenAysnc(refreshTokenId);
	}
}
