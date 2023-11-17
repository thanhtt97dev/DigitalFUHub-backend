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
		public void UpdateUserBankStatus(UserBank userBank) => BankDAO.Instance.UpdateUserBankStatus(userBank);

		public List<DepositTransaction> GetDepositTransaction(int userId, long depositTransactionId, DateTime fromDate, DateTime toDate, int status) => BankDAO.Instance.GetDepositTransaction(userId, depositTransactionId, fromDate, toDate, status);
		public List<DepositTransaction> GetDepositTransactionSucess(long depositTransactionId, string email, DateTime fromDate, DateTime toDate) => BankDAO.Instance.GetDepositTransactionSucess(depositTransactionId, email, fromDate, toDate);

		public List<WithdrawTransaction> GetWithdrawTransaction(int userId, long withdrawTransactionId, DateTime fromDate, DateTime toDate, int status) => BankDAO.Instance.GetWithdrawTransaction(userId, withdrawTransactionId, fromDate, toDate, status);

		public List<WithdrawTransaction> GetAllWithdrawTransaction(long withdrawTransactionId, string email, DateTime fromDate, DateTime toDate, long bankId, string creditAccount, int status) => BankDAO.Instance.GetAllWithdrawTransaction(withdrawTransactionId, email, fromDate, toDate, bankId, creditAccount,  status);
		public WithdrawTransaction? GetWithdrawTransaction(long withdrawTransactionId) => BankDAO.Instance.GetWithdrawTransaction(withdrawTransactionId);
		public WithdrawTransactionBill? GetWithdrawTransactionBill(long withdrawTransactionId) => BankDAO.Instance.GetWithdrawTransactionBill(withdrawTransactionId);

		public void UpdateWithdrawTransactionPaid(long transactionId) => BankDAO.Instance.UpdateWithdrawTransaction(transactionId);

		public string UpdateListWithdrawTransactionPaid(List<long> transactionIds) => BankDAO.Instance.UpdateListWithdrawTransactionPaid(transactionIds);

		public void RejectWithdrawTransaction(long withdrawTransactionId, string? note) => BankDAO.Instance.RejectWithdrawTransaction(withdrawTransactionId, note);

		public (int, long) GetDataWithdrawTransactionMakedToday(long userId) => BankDAO.Instance.GetDataWithdrawTransactionMakedToday(userId);

		public int GetNumberDepositTransactionMakedInToday(long userId) => BankDAO.Instance.GetNumberDepositTransactionMakedInToday(userId);

	}
}
