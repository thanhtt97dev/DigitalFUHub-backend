using BusinessObject;
using BusinessObject.Entities;
using Comons;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
	internal class TransactionCoinDAO
	{
		private static TransactionCoinDAO? instance;
		private static readonly object instanceLock = new object();

		public static TransactionCoinDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new TransactionCoinDAO();
					}
				}
				return instance;
			}
		}

		internal int GetNumberTransactionCoin(long orderId, string email, DateTime? fromDate, DateTime? toDate, int transactionCoinTypeId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transactions = context.TransactionCoin
								.Include(x => x.User)
								.Where
								(x =>
									((fromDate != null && toDate != null) ? fromDate <= x.DateCreate && toDate >= x.DateCreate : true) &&
									x.User.Email.Contains(email) &&
									(orderId == 0 ? true : x.OrderId == orderId) &&
									(transactionCoinTypeId == 0 ? true : x.TransactionCoinTypeId == transactionCoinTypeId)
								)
								.Count();
				return transactions;
			}
		}

		internal List<TransactionCoin> GetHistoryTransactionCoin(long orderId, string email, DateTime? fromDate, DateTime? toDate, int transactionCoinTypeId, int page)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transactions = context.TransactionCoin
								.Include(x => x.User)
								.Where
								(x =>
									((fromDate != null && toDate != null) ? fromDate <= x.DateCreate && toDate >= x.DateCreate : true) &&
									x.User.Email.Contains(email) &&
									(orderId == 0 ? true : x.OrderId == orderId) &&
									(transactionCoinTypeId == 0 ? true : x.TransactionCoinTypeId == transactionCoinTypeId)
								)
								.OrderByDescending(x => x.DateCreate)
								.Skip((page - 1) * Constants.PAGE_SIZE)
								.Take(Constants.PAGE_SIZE)
								.ToList();
				return transactions;
			}
		}

		internal List<TransactionCoin> GetDataReportTransactionCoin(long orderId, string email, DateTime? fromDate, DateTime? toDate, int transactionCoinTypeId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transactions = context.TransactionCoin
								.Include(x => x.User)
								.Where
								(x =>
									((fromDate != null && toDate != null) ? fromDate <= x.DateCreate && toDate >= x.DateCreate : true) &&
									x.User.Email.Contains(email) &&
									(orderId == 0 ? true : x.OrderId == orderId) &&
									(transactionCoinTypeId == 0 ? true : x.TransactionCoinTypeId == transactionCoinTypeId)
								)
								.OrderByDescending(x => x.DateCreate)
								.ToList();
				return transactions;
			}
		}
	}
}
