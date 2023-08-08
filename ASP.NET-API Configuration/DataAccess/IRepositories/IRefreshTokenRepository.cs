using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;


namespace DataAccess.IRepositories
{
	public interface IRefreshTokenRepository
	{
		Task AddRefreshTokenAsync(RefreshToken refreshToken);

		Task<RefreshToken?> GetRefreshToken(string? refreshTokenId);	

		Task RemoveRefreshTokenAysnc(string? refreshTokenId);

	}
}
