using BusinessObject;
using BusinessObject.Entities;
using Comons;
using DTOs.MbBank;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
	public class BankDAO
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
			using (DatabaseContext context = new DatabaseContext())
			{
				var banks = context.Bank.Where(x => x.isActivate).ToList();
				return banks;
			}
		}

		internal List<UserBank> GetAllBankInfoUserLinked(int userId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var userBanks = context.UserBank.Where(x => x.UserId == userId).ToList();
				return userBanks;
			}
		}

		internal int TotalUserLinkedBank(int userId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var total = context.UserBank.Where(x => x.UserId == userId && x.isActivate).Count();
				return total;
			}
		}

		internal void AddUserBank(UserBank userBank)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				context.UserBank.Add(userBank);
				context.SaveChanges();
			}
		}

		internal UserBank? GetUserBank(int userId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var bank = context.UserBank.Include(x => x.Bank).FirstOrDefault(x => x.UserId == userId && x.isActivate);
				return bank;
			}
		}

		internal void CreateDepositTransaction(DepositTransaction transaction)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				transaction.RequestDate = DateTime.Now;
				transaction.PaidDate = null;
				transaction.IsPay = false;
				transaction.UserId = transaction.UserId;
				context.DepositTransaction.Add(transaction);
				context.SaveChanges();
			}
		}

		internal void CreateWithdrawTransaction(WithdrawTransaction transaction)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				// get user bank account
				var userBank = context.UserBank.FirstOrDefault(x => x.UserId == transaction.UserId && x.isActivate);
				if (userBank == null) throw new Exception();
				// add request withdraw
				transaction.UserBankId = userBank.UserBankId;
				transaction.RequestDate = DateTime.Now;
				transaction.PaidDate = null;
				transaction.WithdrawTransactionStatusId = Constants.WITHDRAW_TRANSACTION_IN_PROCESSING;
				transaction.UserId = transaction.UserId;
				context.WithdrawTransaction.Add(transaction);
				// update account balance
				var user = context.User.First(x => x.UserId == transaction.UserId);
				user.AccountBalance = user.AccountBalance - transaction.Amount;
				context.SaveChanges();
			}
		}

		#region Checking Credit Transactions
		public void CheckingCreditTransactions(List<TransactionHistory> transactionHistoryCreditList)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transaction = context.Database.BeginTransaction();
				try
				{
					foreach (var item in transactionHistoryCreditList)
					{
						var deposit = context.DepositTransaction
								.FirstOrDefault(x => string.Equals(x.Code.ToLower(), item.description.ToLower()) &&
								x.Amount == item.creditAmount);
						if (deposit != null)
						{
							if (deposit.IsPay == true) continue;
							//update sataus recharge
							deposit.IsPay = true;
							deposit.PaidDate = System.DateTime.Now;
							//update accout balace user
							var user = context.User.First(x => x.UserId == deposit.UserId);
							user.AccountBalance = user.AccountBalance + item.creditAmount;
						}
					}
					context.SaveChanges();
					transaction.Commit();
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					throw new Exception(ex.Message);
				}
			}
		}
		#endregion

		#region Checking debit transactions
		/// <summary>
		///	 This function this use for adding bill's info when transaction has been withdrawn
		/// </summary>
		/// <param name="transactionHistoryDebitList"></param>
		/// <exception cref="Exception"></exception>
		public void CheckingDebitTransactions(List<TransactionHistory> transactionHistoryDebitList)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transaction = context.Database.BeginTransaction();
				try
				{
					foreach (var item in transactionHistoryDebitList)
					{
						var withdraw = context.WithdrawTransaction
								.FirstOrDefault(x => item.description.ToLower().Contains(x.Code.ToLower()) &&
								x.Amount == item.debitAmount);

						if (withdraw != null)
						{
							if (withdraw.WithdrawTransactionStatusId == Constants.WITHDRAW_TRANSACTION_PAID) continue;
							// set status is paid
							withdraw.WithdrawTransactionStatusId = Constants.WITHDRAW_TRANSACTION_PAID;
							withdraw.PaidDate = item.transactionDate;
							// add bill info
							WithdrawTransactionBill withdrawTransactionBill = new WithdrawTransactionBill()
							{
								WithdrawTransactionId = withdraw.WithdrawTransactionId,
								PostingDate = item.postingDate,
								TransactionDate = item.transactionDate,
								AccountNo = item.accountNo,
								CreditAmount = item.creditAmount,
								DebitAmount = item.creditAmount,
								Currency = item.currency,
								Description = item.description,
								AvailableBalance = item.availableBalance,
								BeneficiaryAccount = item.beneficiaryAccount,
								RefNo = item.refNo,
								BenAccountNo = item.benAccountNo,
								BenAccountName = item.benAccountName,
								BankName = item.bankName,
								DueDate = item.dueDate,
								DocId = item.docId,
								TransactionType = item.transactionType,
							};
							context.WithdrawTransactionBill.Add(withdrawTransactionBill);
						}
					}
					context.SaveChanges();
					transaction.Commit();
				}
				catch (Exception ex)
				{
					transaction.Rollback();
					throw new Exception(ex.Message);
				}
			}
		}
		#endregion

		internal void UpdateUserBankStatus(UserBank userBankUpdate)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var userBank = context.UserBank.FirstOrDefault(x => x.UserId == userBankUpdate.UserId);
				if (userBank == null) throw new Exception("User's bank account not existed!");
				userBank.UpdateAt = DateTime.Now;
				userBank.isActivate = false;
				context.SaveChanges();
			}
		}

		internal List<DepositTransaction> GetDepositTransaction(int userId, long depositTransactionId, DateTime fromDate, DateTime toDate, int status)
		{
			List<DepositTransaction> deposits = new List<DepositTransaction>();
			using (DatabaseContext context = new DatabaseContext())
			{
				deposits = context.DepositTransaction
							.Where(x => x.UserId == userId && fromDate <= x.RequestDate && toDate >= x.RequestDate)
							.OrderByDescending(x => x.RequestDate).ToList();

				if (depositTransactionId != 0)
				{
					deposits = deposits.Where(x => x.DepositTransactionId == depositTransactionId).ToList();
				}
				if (status != 0)
				{
					deposits = deposits.Where(x => x.IsPay == (status == 1)).ToList();
				}

			}
			return deposits;
		}

		internal List<DepositTransaction> GetDepositTransactionSucess(long depositTransactionId, string email, DateTime fromDate, DateTime toDate)
		{
			List<DepositTransaction> deposits = new List<DepositTransaction>();
			using (DatabaseContext context = new DatabaseContext())
			{
				deposits = context.DepositTransaction
							.Include(x => x.User)
							.Where(x => fromDate <= x.RequestDate && toDate >= x.RequestDate && x.User.Email.Contains(email) && x.IsPay)
							.OrderByDescending(x => x.RequestDate).ToList();

				if (depositTransactionId != 0)
				{
					deposits = deposits.Where(x => x.DepositTransactionId == depositTransactionId).ToList();
				}
			}
			return deposits;
		}

		internal List<WithdrawTransaction> GetWithdrawTransaction(int userId, long withdrawTransactionId, DateTime fromDate, DateTime toDate, int status)
		{
			List<WithdrawTransaction> withdraws = new List<WithdrawTransaction>();
			using (DatabaseContext context = new DatabaseContext())
			{
				withdraws = context.WithdrawTransaction
							.Include(x => x.User)
							.Include(x => x.UserBank)
							.ThenInclude(x => x.Bank)
							.Where(x => x.UserId == userId && fromDate <= x.RequestDate && toDate >= x.RequestDate)
							.OrderByDescending(x => x.RequestDate).ToList();

				if (withdrawTransactionId != 0)
				{
					withdraws = withdraws.Where(x => x.WithdrawTransactionId == withdrawTransactionId).ToList();
				}
				if (status != 0)
				{
					withdraws = withdraws.Where(x => x.WithdrawTransactionStatusId == status).ToList();
				}

			}
			return withdraws;
		}

		internal List<WithdrawTransaction> GetAllWithdrawTransaction(long withdrawTransactionId, string email, DateTime fromDate, DateTime toDate, long bankId, string creditAccount, int status)
		{
			List<WithdrawTransaction> withdraws = new List<WithdrawTransaction>();
			using (DatabaseContext context = new DatabaseContext())
			{
				withdraws = context.WithdrawTransaction
							.Include(x => x.User)
							.Include(x => x.UserBank)
							.ThenInclude(x => x.Bank)
							.Where(x => 
								fromDate <= x.RequestDate && toDate >= x.RequestDate && 
								x.User.Email.Contains(email) &&
								x.UserBank.CreditAccount.Contains(creditAccount)
								) 
							.OrderByDescending(x => x.RequestDate).ToList();

				if (withdrawTransactionId != 0)
				{
					withdraws = withdraws.Where(x => x.WithdrawTransactionId == withdrawTransactionId).ToList();
				}
				if (status != 0)
				{
					withdraws = withdraws.Where(x => x.WithdrawTransactionStatusId == status).ToList();
				}
				if(bankId != 0) 
				{
					withdraws = withdraws.Where(x => x.UserBank.BankId == bankId).ToList();	
				}

			}
			return withdraws;
		}

		internal WithdrawTransaction? GetWithdrawTransaction(long withdrawTransactionId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transaction = context.WithdrawTransaction.FirstOrDefault(x => x.WithdrawTransactionId == withdrawTransactionId);
				return transaction;
			}
		}

		internal WithdrawTransactionBill? GetWithdrawTransactionBill(long withdrawTransactionId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var bill = context.WithdrawTransactionBill.FirstOrDefault(x => x.WithdrawTransactionId == withdrawTransactionId);
				return bill;
			}
		}

		internal void UpdateWithdrawTransaction(long transactionId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var withDrawTransaction = context.WithdrawTransaction.FirstOrDefault(x => x.WithdrawTransactionId == transactionId);
				if (withDrawTransaction == null) throw new Exception();
				withDrawTransaction.WithdrawTransactionStatusId = Constants.WITHDRAW_TRANSACTION_PAID;
				withDrawTransaction.PaidDate = DateTime.Now;
				context.SaveChanges();
			}
		}

		internal string UpdateListWithdrawTransactionPaid(List<long> transactionIds)
		{
			string RESPONSE_CODE_SUCCESS = "00";
			string RESPONSE_CODE_FAILD = "03";
			string RESPONSE_CODE_DATA_NOT_FOUND = "02";
			string RESPONSE_CODE_BANK_WITHDRAW_PAID = "BANK_01";

			using (DatabaseContext context = new DatabaseContext())
			{
				var transaction = context.Database.BeginTransaction();
				try
				{
					foreach (var transactionId in transactionIds)
					{
						var withDrawTransaction = context.WithdrawTransaction.FirstOrDefault(x => x.WithdrawTransactionId == transactionId);
						if (withDrawTransaction == null)
						{
							transaction.Rollback();
							return RESPONSE_CODE_DATA_NOT_FOUND;
						}
						if (withDrawTransaction.WithdrawTransactionStatusId == Constants.WITHDRAW_TRANSACTION_PAID)
						{
							transaction.Rollback();
							return RESPONSE_CODE_BANK_WITHDRAW_PAID;
						}
						withDrawTransaction.WithdrawTransactionStatusId = Constants.WITHDRAW_TRANSACTION_PAID;
						withDrawTransaction.PaidDate = DateTime.Now;
					}
					transaction.Commit();
					context.SaveChanges();
					return RESPONSE_CODE_SUCCESS;
				}
				catch (Exception)
				{
					transaction.Rollback();
					return RESPONSE_CODE_FAILD;
				}
			}

		}

		internal List<Transaction> GetHistoryTransactionInternal(long orderId, string email, DateTime fromDate, DateTime toDate, int transactionTypeId)
		{
			List<Transaction> transactions = new List<Transaction>();
			using (DatabaseContext context = new DatabaseContext())
			{
				transactions = context.Transaction
								.Include(x => x.User)
								//.Include(x => x.Order)	
								.Where(x => fromDate <= x.DateCreate && toDate >= x.DateCreate && x.User.Email.Contains(email))
								.OrderByDescending(x => x.DateCreate).ToList();

				if (orderId != 0)
				{
					transactions = transactions.Where(x => x.OrderId == orderId).ToList();
				}
				if (transactionTypeId != 0)
				{
					transactions = transactions.Where(x => x.TransactionTypeId == transactionTypeId).ToList();
				}

			}
			return transactions;
		}
	}
}
