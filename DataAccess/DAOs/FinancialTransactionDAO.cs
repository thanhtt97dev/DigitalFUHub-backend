using BusinessObject;
using DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
	public class FinancialTransactionDAO
	{
		private static FinancialTransactionDAO? instance;
		private static readonly object instanceLock = new object();

		public static FinancialTransactionDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new FinancialTransactionDAO();
					}
				}
				return instance;
			}
		}

		internal void CreateDepositTransaction(DepositTransaction transaction)
		{
			using (ApiContext context = new ApiContext())
			{
				transaction.RequestDate = DateTime.Now;
				transaction.PaidDate = null;
				transaction.IsPay = false;
				transaction.UserId = transaction.UserId;
				context.DepositTransaction.Add(transaction);
				context.SaveChanges();	
			}
		}

		public void CheckingCreditTransactions(List<TransactionHistory> transactionHistoryCreditList)
		{
			using (ApiContext context = new ApiContext())
			{
				var transaction = context.Database.BeginTransaction();
				try
				{
					foreach (var item in transactionHistoryCreditList)
					{
						var isTransactionExisted = context.DepositTransaction
							.FirstOrDefault(x => item.description.Contains(x.Code) && x.Amount == item.creditAmount) != null;
						if (isTransactionExisted)
						{
							var deposit = context.DepositTransaction
								.First(x => item.description.Contains(x.Code) && x.Amount == item.creditAmount);
							if (deposit.IsPay == true) continue;
							//update sataus recharge
							deposit.IsPay = true;
							deposit.PaidDate = System.DateTime.Now;
							//update accout balace user
							var user = context.User.First(x => x.UserId == deposit.UserId);
							user.CustomerBalance = user.CustomerBalance + item.creditAmount;
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
		
	}
}
