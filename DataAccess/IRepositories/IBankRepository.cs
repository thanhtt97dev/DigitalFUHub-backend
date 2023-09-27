using BusinessObject.Entities;
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
		void UpdateUserBank(UserBank userBank);
		UserBank? GetUserBank(int userId);
		void CreateDepositTransaction(DepositTransaction transaction);
		void CreateWithdrawTransaction(WithdrawTransaction transaction);
		List<DepositTransaction> GetDepositTransaction(int userId, long depositTransactionId, DateTime fromDate, DateTime toDate, int status);
	}
}
