using BusinessObject;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
	internal class TwoFactorAuthenticationDAO
	{
		private static TwoFactorAuthenticationDAO? instance;
		private static readonly object instanceLock = new object();
		public static TwoFactorAuthenticationDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new TwoFactorAuthenticationDAO();
					}
				}
				return instance;
			}
		}

		internal string Get2FAKey(long userId)
		{
			using (ApiContext context = new ApiContext())
			{
				var twoFA = context.TwoFactorAuthentication.FirstOrDefault(x => x.UserId == userId);
				if (twoFA == null) throw new Exception("2FA is not existed!");
				return twoFA.SecretKey;
			}
		}

		internal void Add2FAKey(long userId, string key)
		{
			using (ApiContext context = new ApiContext())
			{
				var twoFA = context.TwoFactorAuthentication.FirstOrDefault(x => x.UserId == userId);
				if (twoFA != null) throw new Exception("2FA is existed!");
				TwoFactorAuthentication twoFactorAuthentication = new TwoFactorAuthentication()
				{
					SecretKey = key,
					UserId = userId
				};
				context.TwoFactorAuthentication.Add(twoFactorAuthentication);
				context.SaveChanges();
			}
		}

		internal void Update2FAKey(long userId, string key)
		{
			using (ApiContext context = new ApiContext())
			{
				var twoFA = context.TwoFactorAuthentication.FirstOrDefault(x => x.UserId == userId);
				if (twoFA == null) throw new Exception("2FA is not existed!");
				twoFA.SecretKey = key;
				context.SaveChanges();
			}
		}

		internal void Delete2FAKey(long userId)
		{
			using (ApiContext context = new ApiContext())
			{
				var twoFA = context.TwoFactorAuthentication.FirstOrDefault(x => x.UserId == userId);
				if (twoFA == null) throw new Exception("2FA is not existed!");
				context.TwoFactorAuthentication.Remove(twoFA);	
				context.SaveChanges();
			}
		}
	}
}
