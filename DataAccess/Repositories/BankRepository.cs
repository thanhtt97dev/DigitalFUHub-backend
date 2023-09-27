using BusinessObject.Entities;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class BankRepository : IBankRepository
	{
		public void AddUserBank(UserBank userBank) => BankDAO.Instance.AddUserBank(userBank);
		public List<Bank> GetAll() => BankDAO.Instance.GetAll();

		public List<UserBank> GetAllBankInfoUserLinked(int userId) => BankDAO.Instance.GetAllBankInfoUserLinked(userId);

		public UserBank? GetUserBank(int userId) => BankDAO.Instance.GetUserBank(userId);

		public int TotalUserLinkedBank(int userId) => BankDAO.Instance.TotalUserLinkedBank(userId);

		public void CreateDepositTransaction(DepositTransaction transaction) => BankDAO.Instance.CreateDepositTransaction(transaction);
		public void CreateWithdrawTransaction(WithdrawTransaction transaction) => BankDAO.Instance.CreateWithdrawTransaction(transaction);
		public void UpdateUserBank(UserBank userBank) => BankDAO.Instance.UpdateUserBank(userBank);

		public List<DepositTransaction> GetDepositTransaction(int userId, long depositTransactionId, DateTime fromDate, DateTime toDate, int status) => BankDAO.Instance.GetDepositTransaction(userId, depositTransactionId, fromDate, toDate, status);

		
	}
}
