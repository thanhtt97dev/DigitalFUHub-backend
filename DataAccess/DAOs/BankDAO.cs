using BusinessObject;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
	internal class BankDAO
	{
		private static BankDAO? instance;
		private static readonly object instanceLock = new object();

		public static BankDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new BankDAO();
					}
				}
				return instance;
			}
		}

		internal List<Bank> GetAll()
		{
			using (ApiContext context = new ApiContext())
			{
				var banks = context.Bank.Where(x => x.isActivate).ToList();	
				return banks;
			}
		}

		internal List<UserBank> GetAllBankInfoUserLinked(int userId)
		{
			using (ApiContext context = new ApiContext())
			{
				var userBanks = context.UserBank.Where(x => x.UserId == userId).ToList();
				return userBanks;
			}
		}

		internal int TotalUserLinkedBank(int userId)
		{
			using (ApiContext context = new ApiContext())
			{
				var total = context.UserBank.Where(x => x.UserId == userId).Count();
				return total;
			}
		}

		internal void AddUserBank(UserBank userBank)
		{
			using (ApiContext context = new ApiContext())
			{
				context.UserBank.Add(userBank);
				context.SaveChanges();
			}
		}
	}
}
