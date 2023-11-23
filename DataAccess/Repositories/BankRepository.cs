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
		public int GetNumberDepositTransaction(int userId, long depositTransactionId, string? email, DateTime? fromDate, DateTime? toDate, int status) => BankDAO.Instance.GetNumberDepositTransaction(userId, depositTransactionId, email, fromDate, toDate, status);
		public List<DepositTransaction> GetDepositTransaction(int userId, long depositTransactionId, DateTime? fromDate, DateTime? toDate, int status, int page) => BankDAO.Instance.GetDepositTransaction(userId, depositTransactionId, fromDate, toDate, status, page);
		public List<DepositTransaction> GetDepositTransactionSucess(long depositTransactionId, string? email, DateTime? fromDate, DateTime? toDate, int page) => BankDAO.Instance.GetDepositTransactionSucess(depositTransactionId, email, fromDate, toDate, page);
		public int GetNumberWithdrawTransactionWithCondition(int userId, long withdrawTransactionId, DateTime? fromDate, DateTime? toDate, int status) => BankDAO.Instance.GetNumberWithdrawTransactionWithCondition(userId, withdrawTransactionId, fromDate, toDate, status);
		public List<WithdrawTransaction> GetWithdrawTransaction(int userId, long withdrawTransactionId, DateTime? fromDate, DateTime? toDate, int status, int page) => BankDAO.Instance.GetWithdrawTransaction(userId, withdrawTransactionId, fromDate, toDate, status, page);
		public List<WithdrawTransaction> GetAllWithdrawTransaction(long withdrawTransactionId, string email, DateTime fromDate, DateTime toDate, long bankId, string creditAccount, int status) => BankDAO.Instance.GetAllWithdrawTransaction(withdrawTransactionId, email, fromDate, toDate, bankId, creditAccount,  status);
		public WithdrawTransaction? GetWithdrawTransaction(long withdrawTransactionId) => BankDAO.Instance.GetWithdrawTransaction(withdrawTransactionId);
		public WithdrawTransactionBill? GetWithdrawTransactionBill(long withdrawTransactionId) => BankDAO.Instance.GetWithdrawTransactionBill(withdrawTransactionId);
		public void UpdateWithdrawTransactionPaid(long transactionId) => BankDAO.Instance.UpdateWithdrawTransaction(transactionId);
		public void UpdateWithdrawTransactionCancel(int id) => BankDAO.Instance.UpdateWithdrawTransactionCancel(id);
		public string UpdateListWithdrawTransactionPaid(List<long> transactionIds) => BankDAO.Instance.UpdateListWithdrawTransactionPaid(transactionIds);
		public void RejectWithdrawTransaction(long withdrawTransactionId, string? note) => BankDAO.Instance.RejectWithdrawTransaction(withdrawTransactionId, note);
		public (int, long) GetDataWithdrawTransactionMakedToday(long userId) => BankDAO.Instance.GetDataWithdrawTransactionMakedToday(userId);
		public int GetNumberDepositTransactionMakedInToday(long userId) => BankDAO.Instance.GetNumberDepositTransactionMakedInToday(userId);

		
	}
}
