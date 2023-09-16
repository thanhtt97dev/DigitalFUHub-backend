using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
	public interface IBankRepository
	{
		List<Bank> GetAll();
		List<UserBank> GetAllBankInfoUserLinked(int userId);
		int TotalUserLinkedBank(int userId);
		void AddUserBank(UserBank userBank);
	}
}
