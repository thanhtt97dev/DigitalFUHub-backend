using DataAccess.DAOs;
using DataAccess.IRepositories;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
	public class TwoFactorAuthenticationRepository : ITwoFactorAuthenticationRepository
	{
		public string Get2FAKey(long userId) => TwoFactorAuthenticationDAO.Instance.Get2FAKey(userId);

		public void Add2FAKey(long userId, string key) => TwoFactorAuthenticationDAO.Instance.Add2FAKey(userId, key);

		public void Update2FAKey(long userId, string key) => TwoFactorAuthenticationDAO.Instance.Update2FAKey(userId, key);

		public void Delete2FAKey(long userId) => TwoFactorAuthenticationDAO.Instance.Delete2FAKey(userId);

	}
}
