using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;

namespace DataAccess.IRepositories
{
    public interface IRefreshTokenRepository
	{
		Task AddRefreshTokenAsync(RefreshToken refreshToken);

		RefreshToken? GetRefreshToken(string? refreshTokenStr);	

		Task RemoveRefreshTokenAysnc(string? refreshTokenId);

	}
}
