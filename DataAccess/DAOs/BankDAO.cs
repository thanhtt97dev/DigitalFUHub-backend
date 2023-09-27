using BusinessObject;
using BusinessObject.Entities;
using DTOs.MbBank;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
				var total = context.UserBank.Where(x => x.UserId == userId).Count();
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
				var bank = context.UserBank.Include(x => x.Bank).FirstOrDefault(x => x.UserId == userId);
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
							// add bill info
							DepositeTransactionBill depositeTransactionBill = new DepositeTransactionBill()
							{
								DepositTransactionId = deposit.DepositTransactionId,
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
							context.DepositeTransactionBill.Add(depositeTransactionBill);
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
								.FirstOrDefault(x => string.Equals(x.Code.ToLower(), item.description.ToLower()) &&
								x.Amount == item.debitAmount);
						if (withdraw != null)
						{
							if (withdraw.IsPay == true) continue;
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

		internal void UpdateUserBank(UserBank userBankUpdate)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var userBank = context.UserBank.FirstOrDefault(x => x.UserId == userBankUpdate.UserId);
				if (userBank == null) throw new Exception("User's bank account not existed!");
				userBank.BankId = userBankUpdate.BankId;	
				userBank.CreditAccount = userBankUpdate.CreditAccount;
				userBank.CreditAccountName = userBankUpdate.CreditAccountName;	
				userBank.UpdateAt = DateTime.Now;
				context.SaveChanges();
			}
		}

		internal List<DepositTransaction> GetDepositTransaction(int userId)
		{
			List <DepositTransaction> deposits = new List<DepositTransaction>();
			using (DatabaseContext context = new DatabaseContext())
			{
				deposits = context.DepositTransaction.Where(x => x.UserId == userId && x.IsPay).ToList();
			}
			return deposits;
		}
	}
}
