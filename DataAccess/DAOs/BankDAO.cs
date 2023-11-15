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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
							.Where(x =>
								fromDate <= x.RequestDate && toDate >= x.RequestDate && 
								(depositTransactionId == 0 ? true : x.DepositTransactionId == depositTransactionId) &&
								(status == 0 ? true : x.IsPay == (status == 1))
								).OrderByDescending(x => x.RequestDate).ToList();
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
							.Where(x => 
								fromDate <= x.RequestDate && toDate >= x.RequestDate && 
								x.User.Email.Contains(email) && x.IsPay && 
								(depositTransactionId == 0 ? true : x.DepositTransactionId == depositTransactionId)
								).OrderByDescending(x => x.RequestDate).ToList();
			}
			return deposits;
		}

		internal List<WithdrawTransaction> GetWithdrawTransaction(int userId, long withdrawTransactionId, DateTime fromDate, DateTime toDate, int status)
		{
			List<WithdrawTransaction> withdraws = new List<WithdrawTransaction>();
			using (DatabaseContext context = new DatabaseContext())
			{
				withdraws = (from withdraw in context.WithdrawTransaction
							 join user in context.User
								 on withdraw.UserId equals user.UserId
							 join userBank in context.UserBank
								 on withdraw.UserBankId equals userBank.UserBankId
							 join bank in context.Bank
								 on userBank.BankId equals bank.BankId
							 where
							 (1 == 1) &&
							 withdraw.UserId == userId &&	
							 fromDate <= withdraw.RequestDate && toDate >= withdraw.RequestDate &&
							 (withdrawTransactionId == 0 ? true : withdraw.WithdrawTransactionId == withdrawTransactionId) &&
							 (status == 0 ? true : withdraw.WithdrawTransactionStatusId == status)
							 select new WithdrawTransaction
							 {
								 WithdrawTransactionId = withdraw.WithdrawTransactionId,
								 UserId = withdraw.UserId,
								 User = new User { Email = user.Email },
								 Amount = withdraw.Amount,
								 Code = withdraw.Code,
								 RequestDate = withdraw.RequestDate,
								 UserBank = new UserBank
								 {
									 CreditAccount = userBank.CreditAccount,
									 CreditAccountName = userBank.CreditAccountName,
									 Bank = new Bank
									 {
										 BankCode = bank.BankCode,
										 BankName = bank.BankName,
									 }
								 },
								 WithdrawTransactionStatusId = withdraw.WithdrawTransactionStatusId,
							 }
							).OrderByDescending(x => x.RequestDate).ToList();
			}
			return withdraws;
		}

		internal List<WithdrawTransaction> GetAllWithdrawTransaction(long withdrawTransactionId, string email, DateTime fromDate, DateTime toDate, long bankId, string creditAccount, int status)
		{
			List<WithdrawTransaction> withdraws = new List<WithdrawTransaction>();
			using (DatabaseContext context = new DatabaseContext())
			{
				withdraws = (from withdraw in context.WithdrawTransaction
							 join user in context.User
								 on withdraw.UserId equals user.UserId
							 join userBank in context.UserBank
								 on withdraw.UserBankId equals userBank.UserBankId
							 join bank in context.Bank
								 on userBank.BankId equals bank.BankId
							 where
							 (1 == 1) &&
							 fromDate <= withdraw.RequestDate && toDate >= withdraw.RequestDate &&
							 user.Email.Contains(email) &&
							 userBank.CreditAccount.Contains(creditAccount) &&
							 (withdrawTransactionId == 0 ? true : withdraw.WithdrawTransactionId == withdrawTransactionId) &&
							 (status == 0 ? true : withdraw.WithdrawTransactionStatusId == status) &&
							 (bankId == 0 ? true : bank.BankId == bankId) 
							 select new WithdrawTransaction
							 {
								WithdrawTransactionId = withdraw.WithdrawTransactionId,
								UserId = withdraw.UserId,
								User = new User { Email = user.Email},
								Amount = withdraw.Amount,	
								Code = withdraw.Code,
								RequestDate = withdraw.RequestDate,
								UserBank = new UserBank 
								{
									CreditAccount = userBank.CreditAccount,
									CreditAccountName = userBank.CreditAccountName,
									Bank = new Bank
									{
										BankCode = bank.BankCode, 
										BankName = bank.BankName,
									}
								},
								 WithdrawTransactionStatusId = withdraw.WithdrawTransactionStatusId,
							 }
							).OrderByDescending(x => x.RequestDate).ToList();
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
		internal void RejectWithdrawTransaction(long withdrawTransactionId, string? note)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transaction = context.Database.BeginTransaction();
				try
				{
					//update withdraw transation status
					var withdrawTransaction = context.WithdrawTransaction.First(x => x.WithdrawTransactionId == withdrawTransactionId);
					withdrawTransaction.WithdrawTransactionStatusId = Constants.WITHDRAW_TRANSACTION_REJECT;
					withdrawTransaction.Note = note;
					context.SaveChanges();

					// refund money to customer
					var customer = context.User.First(x => x.UserId == withdrawTransaction.UserId);
					customer.AccountBalance = customer.AccountBalance + withdrawTransaction.Amount;
					context.SaveChanges();
					transaction.Commit();
				}
				catch(Exception ex) 
				{
					transaction.Rollback();
					throw new Exception(ex.Message);
				}
				
			}
		}

		internal int GetNumberWithdrawTransactionMakedInToday(long userId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var result = context.WithdrawTransaction
											.Where(x => 
											x.UserId == userId && 
											x.RequestDate > DateTime.Now.Date && x.RequestDate < DateTime.Now)
											.Select(x => new {})
											.Count();
				return result;
			}
			
		}

		internal int GetNumberDepositTransactionMakedInToday(long userId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var result = context.DepositTransaction
											.Where(x =>
											x.UserId == userId &&
											x.RequestDate > DateTime.Now.Date && x.RequestDate < DateTime.Now)
											.Select(x => new { })
											.Count();
				return result;
			}
		}
	}
}
