using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
	public interface ITwoFactorAuthenticationRepository
	{
		string Get2FAKey(long userId);
		void Add2FAKey(long userId, string key);
		void Update2FAKey(long userId, string key);
		void Delete2FAKey(long userId);
	}
}
