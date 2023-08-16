using BusinessObject;
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

		User? GetUserByRefreshToken(string? refreshTokenId);
	}
}
