﻿using BusinessObject;
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
	}
}
